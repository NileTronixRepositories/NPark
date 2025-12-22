using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public class RolePermission : Entity<Guid>
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public Role Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;

        private RolePermission()
        { }

        public static RolePermission Create(Guid roleId, Guid permissionId) => new RolePermission()
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId
        };

        public static RolePermission CreateWithID(Guid id, Guid roleId, Guid permissionId) => new RolePermission()
        {
            Id = id,
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}