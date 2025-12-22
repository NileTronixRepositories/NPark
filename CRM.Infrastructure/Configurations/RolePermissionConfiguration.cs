using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Configurations
{
    internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(x => new { x.RoleId, x.PermissionId });
            builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            builder.HasOne(x => x.Role).WithMany(x => x.GetPermissions).HasForeignKey(x => x.RoleId);
            builder.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
        }
    }
}