using System;
using System.Linq;
using System.Threading.Tasks;
using The_MTG_Metagamer_Shared.Builders;
using The_MTG_Metagamer_Shared.Models;
using The_MTG_Metagamer_Shared.Scrapers;
using The_MTG_Metagamer_Shared.Extensions;
using The_MTG_Metagamer_Shared.Services;
using Newtonsoft.Json;
using Dasync.Collections;

namespace The_MTG_Metagamer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"{string.Join(", ", args)}");
            try
            {
                // test a single
                ///var test = await McmClient.GetExactProductAsync("Jace, Vryn's Prodigy // Jace, Telepath Unbound");                
                //var single = new The_MTG_Metagamer_Shared.Models.Single
                //{
                //    Name = "Force of Will",
                //    //Name = "Thalia, Guardian of Thraben",
                //    Copies = 4,
                //    CKD_Price = 1.00
                //};
                //var products = await McmService.CheckSingleAsync(single);
                //foreach (var p in products)
                //{
                //    Console.WriteLine($@"{p.Name}:{p.ExpansionName}|{nameof(p.LOW)}:{p.LOW}|{nameof(p.LOWEX)}:{p.LOWEX}|{nameof(p.LOWFOIL)}:{p.LOWFOIL}|{nameof(p.SELL)}:{p.SELL}|{nameof(p.TREND)}:{p.TREND}|{nameof(p.AVG)}:{p.AVG}");
                //}

                //Console.WriteLine($"Single: {JsonConvert.SerializeObject(products.Where(p => p.TREND > 0)?.OrderBy(p => p.TREND)?.FirstOrDefault())}");
                //Console.ReadKey(true);

                //foreach (var card in test.Product)
                //{
                //    var t2 = await McmClient.GetProductByIdAsync(card.IdProduct);
                //}

                // full run
                //var format = Format.modern;

                foreach (var format in (Format[])Enum.GetValues(typeof(Format)))
                {
                    if (format != Format.legacy && format != Format.modern) continue;


                    var decks = (await Scraper.GetDecksAsync(format)).ToList();

                    await decks.ParallelForEachAsync(async deck =>
                    {
                        var (cards, metashare) = await Scraper.GetCardsAsync(deck.Url);
                        deck.Metashare = metashare;
                        deck.Cards = cards;
                        Console.WriteLine($"Cards: {cards.Select(c => c.Copies).Sum()}, {deck.Name}");
                    });

                    // test deck
                    //var decks = new List<Deck>
                    //{
                    //    new Deck("test", "")
                    //    {
                    //        Cards = new List<Card>()
                    //        {
                    //            new Card()
                    //            {
                    //                CardType = CardType.Creatures,
                    //                CKD_Price = 0,
                    //                MCM_Price = 0,
                    //                Copies = 2,
                    //                Name = "Underground Sea"
                    //            }
                    //        }
                    //    }
                    //};

                    //var singles = decks
                    //    .GetSingles()
                    //    .OrderByDescending(d => d.Copies)
                    //    .ThenBy(d => d.Name);
                    //await McmService.GatherMCMProductsAsync(decks);

                    var singles = await McmService.GetSinglePrizesAsync(decks);
                    ExcelBuilder.Build(decks, singles, format);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}, {ex.GetInnerExeptionsStackTrace()}");
            }
            finally
            {
                Console.WriteLine("Done");
                Console.ReadKey(true);
            }
        }
    }
}
