using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Configuration
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(t => t.StartDate).IsRequired();
            builder.Property(t => t.EndDate).IsRequired();
            builder.Property(t => t.Price).IsRequired();
            builder.Property(t => t.UniqueGuidPart)
                  .IsRequired()
                  .HasColumnType("BINARY(4)");
            builder.Property(t => t.ExceedPrice).IsRequired().HasDefaultValue(0);
            builder.Property(t => t.IsCollected).IsRequired().HasDefaultValue(false);
            builder.HasIndex(x => x.UniqueGuidPart).IsUnique();
        }
    }
}