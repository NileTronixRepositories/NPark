using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Configurations
{
    internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
            builder.Property(x => x.NameEn).IsRequired(false).HasMaxLength(100);
            builder.Property(x => x.DescriptionAr).IsRequired(false).HasMaxLength(100);
            builder.Property(x => x.DescriptionEn).IsRequired(false).HasMaxLength(100);
            builder.HasIndex(x => x.NameEn).IsUnique();
            builder.HasMany(x => x.SiteProducts).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
        }
    }
}