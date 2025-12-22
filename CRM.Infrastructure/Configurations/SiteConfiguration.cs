using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Configurations
{
    internal sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
    {
        public void Configure(EntityTypeBuilder<Site> builder)
        {
            builder.ToTable("Sites");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
            builder.Property(x => x.NameAr).IsRequired(false).HasMaxLength(100);
            builder.HasOne(x => x.Account).WithMany(x => x.Sites).HasForeignKey(x => x.AccountId);
            builder.HasMany(x => x.SiteProducts).WithOne(x => x.Site).HasForeignKey(x => x.SiteId);
        }
    }
}