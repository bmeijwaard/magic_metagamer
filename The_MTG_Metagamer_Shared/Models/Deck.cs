using System;
using System.Collections.Generic;
using System.Text;

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
        public IEnumerable<Card> Cards;
    }
}
