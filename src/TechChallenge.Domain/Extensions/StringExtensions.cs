namespace TechChallenge.Domain.Extensions
{
    public static class StringExtensions
    {
        #region Extension Methods

        public static bool IsNullOrEmpty(this string source)
            => string.IsNullOrEmpty(source);

        public static bool IsNullOrWhiteSpace(this string source)
            => string.IsNullOrWhiteSpace(source);

        #endregion
    }
}
