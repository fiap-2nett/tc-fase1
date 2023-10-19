using System;
using System.Collections.Generic;
using System.Linq;

namespace TechChallenge.Domain.Extensions
{
    public static class CollectionExtensions
    {
        #region Extension Methods

        public static bool IsNullOrEmpty<TObject>(this IEnumerable<TObject> enumerable)
            => enumerable is not null ? !enumerable.Any() : true;

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
            => source.Select((item, index) => (item, index));

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            => source.Where(predicate).Select((item, index) => (item, index));

        #endregion
    }
}
