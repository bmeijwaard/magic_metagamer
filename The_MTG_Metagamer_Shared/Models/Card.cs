using System;
using System.Collections.Generic;
using System.Text;

namespace The_MTG_Metagamer_Shared.Models
{
    public class Card
    {
        public string Name;
        public int Copies;
        public CardType CardType;
        public double CKD_Price;
        public double MCM_Price;
        public double TIX_Price;
    }
}
