using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using The_MTG_Metagamer_Shared.Clients;
using The_MTG_Metagamer_Shared.Models;

namespace The_MTG_Metagamer_Shared.Scrapers
{
    public static class Scraper
    {
        public static async Task<IEnumerable<Deck>> GetDecksAsync(Format format)
        {
            var decks = new ConcurrentBag<Deck>();
            var html = await new HtmlClient($"https://www.mtggoldfish.com/metagame/{format.ToString()}/full").MakeRequestAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode
                .SelectNodes("//div[contains(@class,'archetype-tile-description-wrapper')]/div")
                .Distinct()
                .ToList();
            
            foreach (var node in nodes)
            {
                var anchor = node.SelectNodes(".//a")?.FirstOrDefault();
                var href = anchor?.Attributes["href"]?.Value;
                var name = WebUtility.HtmlDecode(anchor?.InnerHtml);
                decks.Add(new Deck(name, $"https://www.mtggoldfish.com{href}"));
            };

            return decks;
        }

        public static async Task<(IEnumerable<Card> Cards, string Metashare)> GetCardsAsync(string deckUrl)
        {
            var cards = new ConcurrentBag<Card>();
            var html = await new HtmlClient(deckUrl).MakeRequestAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var rows = doc.DocumentNode?
                .SelectNodes("//table[contains(@class, 'deck-view-deck-table')]")?
                .Skip(2)?.FirstOrDefault()?
                .SelectNodes("tr");

            if (rows == null) return (cards, string.Empty);

            var metashare = doc.DocumentNode?
                .Descendants("p")?
                .FirstOrDefault(n => n.InnerText.Contains("Decks"))?
                .InnerText;

            var cardType = CardType.Creatures;
            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td");

                if (cells.Count == 1)
                {
                    foreach (CardType type in (CardType[])Enum.GetValues(typeof(CardType)))
                    {
                        if (cells.FirstOrDefault()?.InnerHtml?.Contains(type.ToString()) ?? false)
                            cardType = type;
                    }
                }

                if (cells.Count != 4) continue;
                var card = new Card
                {
                    CardType = cardType
                };

                foreach (var cell in cells)
                {
                    try
                    {
                        if (cell.HasClass("deck-col-qty") && !string.IsNullOrWhiteSpace(cell.InnerHtml))
                            card.Copies = int.Parse(cell.InnerHtml);

                        if (cell.HasClass("deck-col-price") && !string.IsNullOrWhiteSpace(cell.InnerHtml))
                            card.CKD_Price = Convert.ToDouble(decimal.Parse(WebUtility.HtmlDecode(cell.InnerHtml).Replace(",", "")) / card.Copies) / 100;

                        if (cell.HasClass("deck-col-card") && !string.IsNullOrWhiteSpace(cell.InnerHtml))
                        {
                            var anchor = cell.SelectNodes("a")?.FirstOrDefault();
                            card.Name = WebUtility.HtmlDecode(anchor.InnerHtml);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{cell.InnerHtml}, {e.Message}");
                    }
                }

                cards.Add(card);
            }

            return (cards, metashare);
        }
    }
}
