using System;
using System.Collections.Generic;
using System.Linq;
using The_MTG_Metagamer_Shared.Models;

namespace The_MTG_Metagamer_Shared.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            HashSet<TResult> set = new HashSet<TResult>();

            foreach (var item in source)
            {
                var selectedValue = selector(item);

                if (set.Add(selectedValue))
                    yield return item;
            }
        }

        public static IEnumerable<Models.Single> GetSingles(this IEnumerable<Deck> decks)
        {
            foreach (var cardGroup in decks.SelectMany(d => d.Cards).GroupBy(c => c.Name))
            {
                var amountPlayed = cardGroup.Sum(c => c.Copies);
                var ckdPrice = cardGroup.First().CKD_Price;
                yield return new Models.Single(cardGroup.Key, ckdPrice, amountPlayed);
            }
        }
    }
}
