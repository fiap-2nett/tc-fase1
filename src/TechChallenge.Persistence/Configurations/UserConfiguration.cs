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
            var users = new List<(int Id, string Name, string Surname, Email Email, UserRoles Role, string PasswordHash)>()
            {
                new (10_000, "Administrador",   "(built-in)",   Email.Create("admin@techchallenge.app"),    UserRoles.Administrator,    @"MUKOsLOjfoh4YY1ZZLlp+CTyODjmgHhvPAp7PxFiCAWgXo1wibTbOrqht1UhnQi1"), //Password: Admin@123
                new (10_001, "Ailton",          "(built-in)",   Email.Create("ailton@techchallenge.app"),   UserRoles.General,          @"LFhLAgFT8oinF3iXkk63ccZhEllpvGtr/OHG28On+hqniGeX+AIYe8UhNnqztEIm"), //Password: Ailton@123
                new (10_002, "Bruno",           "(built-in)",   Email.Create("bruno@techchallenge.app"),    UserRoles.Analyst,          @"yobUq3aH9/R2x//xYdfaxqX2+FVBBLKzLipbFZILjsTo2sJ9cU/f2F4q6vvwIRzs"), //Password: Bruno@123
                new (10_003, "Cecília",         "(built-in)",   Email.Create("cecilia@techchallenge.app"),  UserRoles.General,          @"LSHTSlFvEBDMS0tjoK2po682H7rLfgL2sXssgm/djzWWouzW4lIydGie7PbmX/1P"), //Password: Cecilia@123
                new (10_004, "Cesar",           "(built-in)",   Email.Create("cesar@techchallenge.app"),    UserRoles.Analyst,          @"q1EyG7yB1S6Cwm7DGuDo3P8ZraEvVHTdBbKHZ1QW3TMG5JWVCnb3EO3UslYiiGeL"), //Password: Cesar@123
                new (10_005, "Paulo",           "(built-in)",   Email.Create("paulo@techchallenge.app"),    UserRoles.General,          @"XAro1VAlABuvkw5sxcSPEUdCeuTZRcM+9qLOumd79674Ro2V0bvvnlgb3zIkA7Yt"), //Password: Paulo@123
            };

            builder.HasData(users.Select(user => new
            {
                user.Id,
                user.Name,
                user.Surname,
                IdRole = (byte)user.Role,
                CreatedAt = DateTime.MinValue.Date,
                IsDeleted = false,
                _passwordHash = user.PasswordHash
            }));

            builder.OwnsOne(p => p.Email).HasData(users.Select(user => new
            {
                UserId = user.Id,
                user.Email.Value
            }));
        }
    }
}
