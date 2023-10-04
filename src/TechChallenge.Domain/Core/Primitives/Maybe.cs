using System;
using System.Threading.Tasks;

namespace TechChallenge.Domain.Core.Primitives
{
    public sealed class Maybe<TValue> : IEquatable<Maybe<TValue>>
    {
        #region Read-Only Fields

        private readonly TValue _value;

        #endregion

        #region Properties

        public bool HasValue => !HasNoValue;
        public bool HasNoValue => _value is null;
        public TValue Value => HasValue
            ? _value
            : throw new InvalidOperationException("The value can not be accessed because it does not exist.");

        #endregion

        #region Constructors

        private Maybe(TValue value)
            => _value = value;

        #endregion

        #region Factory Methods

        public static Maybe<TValue> None => new Maybe<TValue>(default);

        public static Maybe<TValue> From(TValue value) => new Maybe<TValue>(value);

        #endregion

        #region IEquatable Members

        public bool Equals(Maybe<TValue> other)
        {
            if (other is null) return false;
            if (HasNoValue && other.HasNoValue) return true;
            if (HasNoValue || other.HasNoValue) return false;

            return Value.Equals(other.Value);
        }

        #endregion

        #region Operators

        public static implicit operator Maybe<TValue>(TValue value) => From(value);

        public static implicit operator TValue(Maybe<TValue> maybe) => maybe.Value;

        #endregion

        #region Overriden Methods

        public override bool Equals(object obj)
        {
            return obj switch
            {
                null => false,
                TValue value => Equals(new Maybe<TValue>(value)),
                Maybe<TValue> maybe => Equals(maybe),
                _ => false
            };
        }

        public override int GetHashCode() => HasValue ? Value.GetHashCode() : 0;

        #endregion
    }

    public static class MaybeExtensions
    {
        #region Extension Methods

        public static async Task<Maybe<TOutput>> Bind<TInput, TOutput>(this Maybe<TInput> maybe, Func<TInput, Task<Maybe<TOutput>>> func)
            => maybe.HasValue ? await func(maybe.Value) : Maybe<TOutput>.None;

        public static async Task<TOutput> Match<TInput, TOutput>(this Task<Maybe<TInput>> resultTask, Func<TInput, TOutput> onSuccess, Func<TOutput> onFailure)
        {
            Maybe<TInput> maybe = await resultTask;
            return maybe.HasValue ? onSuccess(maybe.Value) : onFailure();
        }

        #endregion
    }
}
