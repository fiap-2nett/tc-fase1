using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Core.Primitives;
using TechChallenge.Persistence.Core.Abstractions;

namespace TechChallenge.Persistence.Core.Primitives
{
    internal abstract class EntitySeedConfiguration<TEntity, TKey> : IEntitySeedConfiguration        
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        #region IEntitySeedConfiguration Members

        public abstract IEnumerable<object> Seed();

        public void Configure(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TEntity>().HasData(Seed());

        #endregion
    }
}
