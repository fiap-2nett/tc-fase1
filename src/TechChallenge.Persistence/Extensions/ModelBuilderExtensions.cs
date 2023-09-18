using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Persistence.Core.Abstractions;

namespace TechChallenge.Persistence.Extensions
{
    internal static class ModelBuilderExtensions
    {
        #region Extension Methods

        public static ModelBuilder RemoveCascadeConvention(this ModelBuilder modelBuilder)
        {
            var allRelationships = modelBuilder.Model.GetEntityTypes()
                .Where(e => !e.IsOwned()).SelectMany(e => e.GetForeignKeys());

            foreach (var relationship in allRelationships)
                relationship.DeleteBehavior = DeleteBehavior.Restrict;

            return modelBuilder;
        }

        public static ModelBuilder SetDefaultColumnTypes(this ModelBuilder modelBuilder)
        {
            var allStringProperties = modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string)));

            foreach (var property in allStringProperties)
                property.SetColumnType("varchar(150)");

            return modelBuilder;
        }

        public static ModelBuilder ApplySeedConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var seedConfigurations = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IEntitySeedConfiguration).IsAssignableFrom(type));

            foreach (var config in seedConfigurations)
            {
                dynamic instance = Activator.CreateInstance(config) ?? throw new InvalidOperationException();
                ((IEntitySeedConfiguration)instance).Configure(modelBuilder);
            }

            return modelBuilder;
        }

        #endregion
    }
}
