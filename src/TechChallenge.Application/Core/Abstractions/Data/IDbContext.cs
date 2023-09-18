using System;
using System.Threading;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Application.Core.Abstractions.Data
{
    public interface IDbContext
    {
        #region IDbContext Members

        DbSet<TEntity> Set<TEntity, TKey>()
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>;

        Task<TEntity> GetBydIdAsync<TEntity, TKey>(TKey idEntity)
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>;

        void Insert<TEntity, TKey>(TEntity entity)
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>;

        void InsertRange<TEntity, TKey>(IReadOnlyCollection<TEntity> entities)            
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>;

        void Remove<TEntity, TKey>(TEntity entity)            
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>;

        Task<int> ExecuteSqlAsync(string sql, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);

        #endregion
    }
}
