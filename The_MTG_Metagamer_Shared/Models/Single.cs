using System;
using System.Collections.Generic;
using System.Text;

namespace The_MTG_Metagamer_Shared.Models
{
    public class Single
    {
        public Single()
        {
        }

        public Single(string name, double CKD_price) : this()
        {
            Name = name;
            CKD_Price = CKD_price;
        }

        public string Name;
        public double CKD_Price;
        public double MCM_Price;
    }
}
