namespace The_MTG_Metagamer_Shared.Clients.McmModels
{
    public class ProductResults
    {
        public Product[] Product { get; set; }
    }

    public class ProductResult
    {
        public Product Product { get; set; }
    }

    public class Product
    {
        public int IdProduct { get; set; }                  // Product ID
        public int IdMetaproduct { get; set; }              // Metaproduct ID
        public int? CountReprints { get; set; }              // Number of total products bundled by the metaproduct
        public string EnName { get; set; }                     // Product's English name
        public string LocName { get; set; }
        public Localization[] Localization { get; set; }               // localization entities for the product's name
        public Category Category { get; set; }                 // Category entity the product belongs to
        public string Website { get; set; }                    // URL to the product (relative to MKM's base URL)
        public string Image { get; set; }                      // Path to the product's image
        public string GameName { get; set; }                   // the game's name
        public string CategoryName { get; set; }               // the category's name
        public string Number { get; set; }                     // Number of product within the expansion (where applicable)
        public string Rarity { get; set; }                     // Rarity of product (where applicable)
        public string ExpansionName { get; set; }              // Expansion's name 
        public int? ExpansionIcon { get; set; }
        public int? CountArticles { get; set; }
        public int? CountFoils { get; set; }
        public Link[] Links { get; set; }                      // HATEOAS links


        // additional?
        public Expansion Expansion { get; set; }               // detailed expansion information (where applicable)
        public PriceGuide PriceGuide { get; set; }             // Price guide entity '''(ATTN: not returned for expansion requests)'''
        public Reprint[] Reprint { get; set; }                   // Reprint entities for each similar product bundled by the metaproduct
    }

    public class Localization
    {
        public string Name { get; set; }
        public int IdLanguage { get; set; }
        public string LanguageName { get; set; }
    }
    public class Link
    {
        public string Rel { get; set; }
		public string Href { get; set; }
        public string Method { get; set; }
    }

    public class Category
    {
        public string IdCategory { get; set; }             // Category ID
        public string CategoryName { get; set; }           // Category's name
    }

    public class Expansion
    {
        public string IdExpansion { get; set; }
        public string EnName { get; set; }
        public int ExpansionIcon { get; set; }
    }

    public class PriceGuide
    {
        public double SELL { get; set; }                   // Average price of articles ever sold of this product
        public double LOW { get; set; }                    // Current lowest non-foil price (all conditions)
        public double LOWEX { get; set; }                  // Current lowest non-foil price (condition EX and better)
        public double LOWFOIL { get; set; }                // Current lowest foil price
        public double AVG { get; set; }                    // Current average non-foil price of all available articles of this product
        public double TREND { get; set; }                  // Current trend price
    }

    public class Reprint
    {
        public string IdProduct { get; set; }
        public string Expansion { get; set; }
        public string ExpIcon { get; set; }
    }
}
