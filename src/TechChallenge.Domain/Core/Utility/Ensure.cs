using System;
using TechChallenge.Domain.Extensions;

namespace TechChallenge.Domain.Core.Utility
{
    public static class Ensure
    {
        #region NotNull Methods

        public static void NotNull<T>(T value, string message, string argumentName)
            where T : class
        {
            if (value is null)
                throw new ArgumentNullException(argumentName, message);
        }

        #endregion

        #region NotEmpty Methods

        public static void NotEmpty(string value, string message, string argumentName)
        {
            if (value.IsNullOrWhiteSpace())
                throw new ArgumentException(message, argumentName);
        }

        public static void NotEmpty(Guid value, string message, string argumentName)
        {
            if (value.IsEmpty())
                throw new ArgumentException(message, argumentName);
        }

        public static void NotEmpty(DateTime value, string message, string argumentName)
        {
            if (value.IsDefault())
                throw new ArgumentException(message, argumentName);
        }

        #endregion

        #region LessThan Methods

        public static void LessThan(int value, int maxValue, string message, string argumentName)
        {
            if (value >= maxValue)
                throw new ArgumentException(message, argumentName);
        }

        #endregion

        #region LessThanOrEqual Methods

        public static void LessThanOrEqual(int value, int maxValue, string message, string argumentName)
        {
            if (value > maxValue)
                throw new ArgumentException(message, argumentName);
        }

        #endregion

        #region GreaterThan Methods

        public static void GreaterThan(int value, int minValue, string message, string argumentName)
        {
            if (value <= minValue)
                throw new ArgumentException(message, argumentName);
        }

        #endregion

        #region GreaterThanOrEqual Methods

        public static void GreaterThanOrEqual(int value, int minValue, string message, string argumentName)
        {
            if (value < minValue)
                throw new ArgumentException(message, argumentName);
        }

        #endregion
    }
}
