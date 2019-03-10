using System;
using System.Collections.Async;
using System.Linq;
using System.Threading.Tasks;
using The_MTG_Metagamer_Shared.Builders;
using The_MTG_Metagamer_Shared.Models;
using The_MTG_Metagamer_Shared.Scrapers;
using The_MTG_Metagamer_Shared.Extensions;
using The_MTG_Metagamer_Shared.Clients;
using System.Collections.Generic;
using The_MTG_Metagamer_Shared.Services;

namespace The_MTG_Metagamer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {

                // test a single
                //var test = await McmClient.GetExactProductAsync("Jace, Vryn's Prodigy // Jace, Telepath Unbound");
                //// Console.ReadKey(true);

                //foreach (var card in test.Product)
                //{
                //    var t2 = await McmClient.GetProductByIdAsync(card.IdProduct);
                //}

                // full run
                // var format = Format.modern;

                foreach (var format in (Format[])Enum.GetValues(typeof(Format)))
                {
                    var decks = await Scraper.GetDecksAsync(format);

                    await decks.ParallelForEachAsync(async deck =>
                    {
                        var cards = await Scraper.GetCardsAsync(deck.Url);
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

                    var singles = decks
                    .SelectMany(d => d.Cards)
                    .Select(c => new The_MTG_Metagamer_Shared.Models.Single(c.Name, c.CKD_Price))
                    .DistinctBy(s => s.Name)
                    .OrderBy(s => s.Name)
                    .ToList();
                    // var singles = await McmService.GetSinglePrizesAsync(decks.ToList());

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
