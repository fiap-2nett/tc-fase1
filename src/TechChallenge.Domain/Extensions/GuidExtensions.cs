using System;

namespace TechChallenge.Domain.Extensions
{
    public static class GuidExtensions
    {
        #region Extension Methods

        public static bool IsEmpty(this Guid source)
            => source == Guid.Empty;

        #endregion
    }
}
