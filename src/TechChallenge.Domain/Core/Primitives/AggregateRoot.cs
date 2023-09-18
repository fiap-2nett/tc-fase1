using System;

namespace TechChallenge.Domain.Core.Primitives
{
    public abstract class AggregateRoot<TKey> : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Constructors

        protected AggregateRoot()
        { }

        protected AggregateRoot(TKey idAggregateRoot)
            : base(idAggregateRoot)
        { }

        #endregion
    }
}
