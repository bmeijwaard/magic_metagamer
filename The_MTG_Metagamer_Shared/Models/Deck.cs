using System;
using System.Collections.Generic;

namespace The_MTG_Metagamer_Shared.Models
{
    public class Deck
    {
        public Deck(string name, string url)
        {
            Url = url;
            Name = name;
            Cards = new List<Card>();
        }
        public string Url;
        public string Name;
        public string Metashare;
        public int Ranking
        {
            get
            {
                var result = string.IsNullOrWhiteSpace(Metashare)
                    ? "0"
                    : Metashare?
                        .Split(new string[] { " Decks " }, StringSplitOptions.None)?[0] ?? "0";

                return int.Parse(result);
            }
        }
        public IEnumerable<Card> Cards;
    }
}
