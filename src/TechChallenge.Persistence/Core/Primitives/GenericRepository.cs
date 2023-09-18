using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Persistence.Core.Primitives
{
    internal abstract class GenericRepository<TEntity, TKey>
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Properties

        protected IDbContext DbContext { get; }

        #endregion

        #region Constructors

        protected GenericRepository(IDbContext dbContext)
            => DbContext = dbContext;

        #endregion

        #region Methods

        public async Task<TEntity> GetByIdAsync(TKey entityId)
            => await DbContext.GetBydIdAsync<TEntity, TKey>(entityId);

        public void Insert(TEntity entity)
            => DbContext.Insert<TEntity, TKey>(entity);

        public void InsertRange(IReadOnlyCollection<TEntity> entities)
            => DbContext.InsertRange<TEntity, TKey>(entities);

        public void Update(TEntity entity)
            => DbContext.Set<TEntity, TKey>().Update(entity);

        public void Remove(TEntity entity)
            => DbContext.Remove<TEntity, TKey>(entity);

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
            => await DbContext.Set<TEntity, TKey>().FirstOrDefaultAsync(predicate);

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
            => await DbContext.Set<TEntity, TKey>().AnyAsync(predicate);

        #endregion
    }
}
