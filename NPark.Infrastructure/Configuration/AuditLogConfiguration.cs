using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Configuration
{
    public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> b)
        {
            b.ToTable("AuditLogs");

            b.HasKey(x => x.Id);

            b.Property(x => x.CreatedAtUtc)
                .IsRequired();

            b.Property(x => x.EventName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.EventCategory)
                .HasMaxLength(100);

            b.Property(x => x.Role)
                .HasMaxLength(100);

            b.Property(x => x.RequestPath)
                .HasMaxLength(400);

            b.Property(x => x.HttpMethod)
                .HasMaxLength(10);

            b.Property(x => x.ErrorCode)
                .HasMaxLength(200);

            b.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            b.Property(x => x.CorrelationId)
                .HasMaxLength(64);

            b.Property(x => x.TraceId)
                .HasMaxLength(64);
        }
    }
}