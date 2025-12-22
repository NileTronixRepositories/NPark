using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Configurations
{
    internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Password).IsRequired().HasMaxLength(100);
            builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
            builder.Property(x => x.NameAr).IsRequired(false).HasMaxLength(100);
            builder.HasOne(x => x.Role).WithMany()
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Sites).WithOne(x => x.Account)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.NameEn);
        }
    }
}