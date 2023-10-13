using System.Collections.Generic;
using System.Linq;

namespace TechChallenge.Domain.Extensions
{
    public static class CollectionExtensions
    {
        #region Extension Methods

        public static bool IsNullOrEmpty<TObject>(this IEnumerable<TObject> enumerable)
            => enumerable is not null ? !enumerable.Any() : true;

        #endregion
    }
}
