using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.ValueObjects;
using TechChallenge.Infrastructure.Cryptography;

namespace TechChallenge.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        private readonly PasswordHasher _passwordHasher = new PasswordHasher();

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.IdRole).IsRequired();
            builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Surname).HasMaxLength(150).IsRequired();
            builder.OwnsOne(p => p.Email, builder =>
            {
                builder.WithOwner();
                builder.Property(email => email.Value)
                    .HasColumnName(nameof(User.Email))
                    .HasMaxLength(Email.MaxLength)
                    .IsRequired();
            });
            builder.Property<string>("_passwordHash").HasField("_passwordHash").HasColumnName("PasswordHash").IsRequired();

            builder.Property(p => p.IsDeleted);
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.LastUpdatedAt);

            builder.HasOne<Role>()
                .WithMany()
                .HasForeignKey(p => p.IdRole).OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(p => !p.IsDeleted);

            SeedBuiltInUsers(builder);
        }

        private void SeedBuiltInUsers(EntityTypeBuilder<User> builder)
        {
            var users = new List<(int Id, string Name, string Surname, Email Email, UserRoles Role, Password Password)>()
            {
                new (10_000, "Administrador",   "(built-in)",   Email.Create("admin@techchallenge.app"),    UserRoles.Administrator,    Password.Create("Admin@123")),
                new (10_001, "Ailton",          "(built-in)",   Email.Create("ailton@techchallenge.app"),   UserRoles.General,             Password.Create("Ailton@123")),
                new (10_002, "Bruno",           "(built-in)",   Email.Create("bruno@techchallenge.app"),    UserRoles.Analyst,          Password.Create("Bruno@123")),
                new (10_003, "Cecília",         "(built-in)",   Email.Create("cecilia@techchallenge.app"),  UserRoles.General,             Password.Create("Cecilia@123")),
                new (10_004, "Cesar",           "(built-in)",   Email.Create("cesar@techchallenge.app"),    UserRoles.Analyst,          Password.Create("Cesar@123")),
                new (10_005, "Paulo",           "(built-in)",   Email.Create("paulo@techchallenge.app"),    UserRoles.General,             Password.Create("Paulo@123")),
            };

            builder.HasData(users.Select(user => new
            {
                user.Id,
                user.Name,
                user.Surname,
                IdRole = (byte)user.Role,
                CreatedAt = DateTime.MinValue.Date,
                IsDeleted = false,
                _passwordHash = _passwordHasher.HashPassword(user.Password)
            }));

            builder.OwnsOne(p => p.Email).HasData(users.Select(user => new
            {
                UserId = user.Id,
                user.Email.Value
            }));
        }
    }
}
