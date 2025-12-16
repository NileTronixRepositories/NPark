using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrafficLicensing.Domain.Entities;

namespace TrafficLicensing.Infrastructure.Configurations
{
    public sealed class ArchiveRequestsConfiguration : IEntityTypeConfiguration<ArchiveRequests>
    {
        public void Configure(EntityTypeBuilder<ArchiveRequests> builder)
        {
            builder.ToTable("ArchiveRequests");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PlateNumber).IsRequired();
            builder.Property(x => x.ActionType).IsRequired();
            builder.Property(x => x.ActionTaken).IsRequired();
            builder.Property(x => x.Note).IsRequired(false);
            builder.Property(x => x.RejectReason).IsRequired(false);
        }
    }
}