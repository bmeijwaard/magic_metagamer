using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using The_MTG_Metagamer_Shared.Clients;
using The_MTG_Metagamer_Shared.Extensions;
using The_MTG_Metagamer_Shared.Models;

namespace The_MTG_Metagamer_Shared.Services
{
    public static class McmService
    {
        public static async Task<List<Models.Single>> GetSinglePrizesAsync(List<Deck> decks)
        {
            var singles = decks
                    .SelectMany(d => d.Cards)
                    .Select(c => new Models.Single(c.Name, c.CKD_Price))
                    .DistinctBy(s => s.Name)
                    .OrderBy(s => s.Name)
                    .ToList();

            var skip = new[] { "Plains", "Island", "Forest", "Mountain", "Swamp", "Wastes" };
            var replacements = new Dictionary<string, string>
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
                };

            var total = singles.Count();
            foreach (var single in singles)
            {
                try
                {
                    if (skip.Contains(single.Name))
                    {
                        single.MCM_Price = 0.10d;
                        continue;
                    }

                    var name = replacements.Keys.Contains(single.Name)
                        ? replacements.FirstOrDefault(s => s.Key == single.Name).Value
                        : single.Name;

                    var products = (await McmClient.GetExactProductAsync(name))?.Product?.Where(p => p.Rarity != "Special" && p.ExpansionName != "International Edition" && p.ExpansionName != "Summer Magic" && p.ExpansionName != "Alpha" && p.ExpansionName != "Beta");
                    double avgTrend = 0.00d;
                    foreach (var p in products)
                    {
                        var product = await McmClient.GetProductByIdAsync(p.IdProduct);
                        if (products == null) continue;
                        avgTrend = avgTrend == 0.00d || product?.Product?.PriceGuide?.TREND < avgTrend ? product.Product.PriceGuide.TREND : avgTrend;
                    }

                    single.MCM_Price = avgTrend;
                    Console.WriteLine($"Done, {total--} remaining, getting mcm price for: {single.Name}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{single.Name}: {e.Message}, {e.GetInnerExeptionsStackTrace()}");
                }
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
    }
}
