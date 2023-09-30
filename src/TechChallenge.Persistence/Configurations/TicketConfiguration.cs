using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;

namespace TechChallenge.Persistence.Configurations
{
    internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("tickets");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id);
            builder.Property(p => p.IdCategory).IsRequired();
            builder.Property(p => p.IdStatus).IsRequired();
            builder.Property(p => p.IdUserRequester).IsRequired();
            builder.Property(p => p.IdUserAssigned);
            builder.Property(p => p.Description).HasColumnType("varchar(max)").IsRequired();

            builder.Property(p => p.CompletedAt);
            builder.Property(p => p.CancellationReason).HasColumnType("varchar(max)");


            builder.Property(p => p.IsDeleted);
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.LastUpdatedBy);
            builder.Property(p => p.LastUpdatedAt);

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.IdCategory).OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<TicketStatus>()
                .WithMany()
                .HasForeignKey(p => p.IdStatus).OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.IdUserRequester).OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.IdUserAssigned).OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.LastUpdatedBy).OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
