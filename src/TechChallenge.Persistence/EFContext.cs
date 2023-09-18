using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Domain.Core.Abstractions;
using TechChallenge.Domain.Core.Primitives;
using TechChallenge.Persistence.Extensions;

namespace TechChallenge.Persistence
{
    public sealed class EFContext : DbContext, IDbContext, IUnitOfWork
    {
        #region Constructors

        public EFContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        { }

        #endregion

        #region  IDbContext Members

        public DbSet<TEntity> Set<TEntity, TKey>()
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>
            => base.Set<TEntity>();

        public async Task<TEntity> GetBydIdAsync<TEntity, TKey>(TKey entityId)
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>
            => await Set<TEntity>().FirstOrDefaultAsync(e => e.Id.Equals(entityId));

        public void Insert<TEntity, TKey>(TEntity entity)
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>
            => Set<TEntity>().Add(entity);

        public void InsertRange<TEntity, TKey>(IReadOnlyCollection<TEntity> entities)
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>
            => Set<TEntity>().AddRange(entities);

        public void Remove<TEntity, TKey>(TEntity entity)
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>
            => Set<TEntity>().Remove(entity);

        public Task<int> ExecuteSqlAsync(string sql, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
            => Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

        #endregion

        #region IUnitOfWork Members

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.Now;

            UpdateAuditableEntities(currentDate);
            UpdateNonDeletableEntities(currentDate);

            return await base.SaveChangesAsync(cancellationToken);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Database.BeginTransactionAsync(cancellationToken);

        #endregion

        #region Overriden Methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetDefaultColumnTypes();
            modelBuilder.RemoveCascadeConvention();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.ApplySeedConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Private Methods

        private void UpdateAuditableEntities(DateTime currentDate)
        {
            foreach (var entityEntry in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entityEntry.State == EntityState.Added)
                    entityEntry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = currentDate;

                if (entityEntry.State == EntityState.Modified)
                    entityEntry.Property(nameof(IAuditableEntity.LastUpdatedAt)).CurrentValue = currentDate;
            }
        }

        private void UpdateNonDeletableEntities(DateTime currentDate)
        {
            foreach (var entityEntry in ChangeTracker.Entries<ISoftDeletableEntity>())
            {
                if (entityEntry.State != EntityState.Deleted)
                    continue;

                entityEntry.Property(nameof(ISoftDeletableEntity.IsDeleted)).CurrentValue = true;
                entityEntry.State = EntityState.Modified;

                UpdateDeletedEntityEntryReferencesToUnchanged(entityEntry);
            }
        }

        private static void UpdateDeletedEntityEntryReferencesToUnchanged(EntityEntry entityEntry)
        {
            if (!entityEntry.References.Any())
                return;

            foreach (var referenceEntry in entityEntry.References.Where(r => r.TargetEntry.State == EntityState.Deleted))
            {
                referenceEntry.TargetEntry.State = EntityState.Unchanged;
                UpdateDeletedEntityEntryReferencesToUnchanged(referenceEntry.TargetEntry);
            }
        }

        #endregion
    }
}
