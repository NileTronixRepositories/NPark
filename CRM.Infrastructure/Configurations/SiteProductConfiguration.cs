using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Configurations
{
    internal sealed class SiteProductConfiguration : IEntityTypeConfiguration<SiteProduct>
    {
        public void Configure(EntityTypeBuilder<SiteProduct> builder)
        {
            builder.ToTable("SiteProducts");
            builder.HasKey(x => new { x.SiteId, x.ProductId });
            builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.HasOne(x => x.Site).WithMany(x => x.SiteProducts).HasForeignKey(x => x.SiteId);
            builder.HasOne(x => x.Product).WithMany(x => x.SiteProducts).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}