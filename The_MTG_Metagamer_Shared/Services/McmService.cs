using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using The_MTG_Metagamer_Shared.Clients;
using The_MTG_Metagamer_Shared.Data.Entities;
using The_MTG_Metagamer_Shared.Extensions;
using The_MTG_Metagamer_Shared.Models;

namespace The_MTG_Metagamer_Shared.Services
{
    public static class McmService
    {
        private static int total = 0;
        private static readonly string[] _skip = new[] { "Plains", "Island", "Forest", "Mountain", "Swamp", "Wastes" };
        private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>
                {
                    { "Search for Azcanta", "Search for Azcanta / Azcanta, the Sunken Ruin" },
                    { "Delver of Secrets", "Delver of Secrets / Insectile Aberration" },
                    { "Huntmaster of the Fells", "Huntmaster of the Fells / Ravager of the Fells" },
                    { "Jace, Vryn's Prodigy", "Jace, Vryn's Prodigy // Jace, Telepath Unbound" },
                    { "Nissa, Vastwood Seer", "Nissa, Vastwood Seer // Nissa, Sage Animist" },
                    { "Thing in the Ice", "Thing in the Ice / Awoken Horror" },
                    { "Hanweir Battlements", "Hanweir Battlements / Hanweir, the Writhing Township" },
                    { "Hanweir Garrison", "Hanweir Garrison / Hanweir, the Writhing Township" },
                    { "Garruk Relentless", "Garruk Relentless / Garruk, the Veil-Cursed" },
                    { "Kytheon, Hero of Akros", "Kytheon, Hero of Akros // Gideon, Battle-Forged" },
                    { "Brazen Borrower", "Brazen Borrower // Petty Theft" },
                    { "Bonecrusher Giant", "Bonecrusher Giant // Stomp" },
                    { "Fae of Wishes", "Fae of Wishes // Granted" },
                    { "Duskwatch Recruiter", "Duskwatch Recruiter / Krallenhorde Howler" },
                    { "Merchant of the Vale", "Merchant of the Vale // Haggle" },
                    { "Lurrus of the Dream Den", "Lurrus of the Dream-Den" },
                };

        private static readonly ProductService _productService;
        static McmService()
        {
            _productService = new ProductService();
        }

        public static async Task<List<Models.Single>> GetSinglePrizesAsync(List<Deck> decks)
        {
            var singles = decks
                 .GetSingles()
                 .OrderByDescending(d => d.Copies)
                 .ThenBy(d => d.Name)
                 .ToList();

            total = singles.Count();
            foreach (var single in singles)
            {
                await ProcessSingleAsync(single);
            }

            foreach (var deck in decks)
            {
                foreach (var card in deck.Cards)
                {
                    card.MCM_Price = singles.FirstOrDefault(s => s.Name == card.Name)?.MCM_Price ?? 0d;
                }
            }

            return singles;
        }

        public static async Task GatherMCMProductsAsync(List<Deck> decks)
        {
            var singles = decks
                 .GetSingles()
                 .OrderByDescending(d => d.Copies)
                 .ThenBy(d => d.Name)
                 .ToList();

            total = singles.Count();
            var count = 0;
            foreach (var single in singles)
            {
                try
                {
                    if (_skip.Contains(single.Name))
                        continue;

                    var products = await CheckSingleAsync(single);
                    count = products.Count();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {single.Name}");
                    Console.WriteLine($"{ex}");
                }
                finally
                {
                    Interlocked.Decrement(ref total);
                    Console.WriteLine($"Done, {total} remaining prices seen {count}, getting mcm price for: {single.Name}");
                }
            }
        }

        public static async Task<IEnumerable<ProductEntity>> CheckSingleAsync(Models.Single single)
        {
            var name = single.NormalizedName();

            var entities = await _productService.GetAsync(p => p.Name.StartsWith(name));
            if (!entities.Any() || entities.Min(p => p.LastUpdated?.Date < DateTime.Now.AddDays(-7).Date))
            {
                var products = await McmClient.GetExactProductAsync(name);
                foreach (var p in products?.Product?.Where(p => p.Rarity != "Special"))
                {
                    var product = await McmClient.GetProductByIdAsync(p.IdProduct);
                    if (product == null)
                        continue;
                    else if (entities.Select(e => e.ProductId).Contains(product.Product.IdProduct))
                        await _productService.UpdateAsync(product.Product);
                    else
                        await _productService.CreateAsync(product.Product);
                }

                entities = await _productService.GetAsync(p => p.Name == name);
            }

            return entities;
        }

        public static async Task ProcessSingleAsync(Models.Single single)
        {
            try
            {
                if (_skip.Contains(single.Name))
                    return;

                double avgTrend = 0.00d;
                try
                {
                    var products = await CheckSingleAsync(single);
                    avgTrend = products?.Where(p => p.TREND > 0.00d)?.OrderBy(p => p.TREND)?.FirstOrDefault()?.TREND ?? 0.00d;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {single.Name}");
                    Console.WriteLine($"{ex}");
                }
                finally
                {
                    Interlocked.Decrement(ref total);
                    Console.WriteLine($"Done, {total}, getting mcm price for: {single.Name}");
                    single.MCM_Price = avgTrend;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{single.Name}: {e.ToString()}");
            }
        }

        private static string NormalizedName(this Models.Single single)
        {
            return _replacements.Keys.Contains(single.Name)
                ? _replacements.FirstOrDefault(s => s.Key == single.Name).Value
                : single.Name;
        }
    }
}
