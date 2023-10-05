using System;

namespace TechChallenge.Domain.Helpers
{
    public static class EnumHelper
    {
        public static bool TryConvert<TEnum>(object value, out TEnum result) where TEnum : Enum
        {
            result = default;

            var isDefined = Enum.IsDefined(typeof(TEnum), value);
            if (isDefined)
                result = (TEnum)Enum.ToObject(typeof(TEnum), value);

            return isDefined;
        }
    }
}
