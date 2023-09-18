using System;
using System.Collections.Generic;
using System.Linq;

namespace TechChallenge.Domain.Core.Primitives
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        #region IEquatable Members

        public bool Equals(ValueObject other)
        {
            if (other is null) return false;
            return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
        }

        #endregion

        #region Abstract Methods

        protected abstract IEnumerable<object> GetAtomicValues();

        #endregion

        #region Overriden Methods

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            if (!(obj is ValueObject valueObject)) return false;

            return GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
        }

        public override int GetHashCode()
            => GetAtomicValues().Aggregate(default(HashCode), (hashCode, obj) =>
            {
                hashCode.Add(obj.GetHashCode());
                return hashCode;
            }).ToHashCode();

        #endregion

        #region Operators

        public static bool operator ==(ValueObject a, ValueObject b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject a, ValueObject b)
            => !(a == b);

        #endregion
    }
}
