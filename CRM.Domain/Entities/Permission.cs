using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public class Permission : Entity<Guid>
    {
        private List<RolePermission> _rolePermissions { get; set; } = new();
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

        private Permission()
        { }

        public static Permission Create(string nameEn, string nameAr, string description) => new Permission()
        {
            NameEn = nameEn,
            NameAr = nameAr,
            Description = description
        };

        public static Permission CreateWithID(Guid id, string nameEn, string nameAr, string description) => new Permission()
        {
            Id = id,
            NameEn = nameEn,
            NameAr = nameAr,
            Description = description
        };

        public void UpdateName(string name) => NameEn = name;

        public void UpdateNameAr(string name) => NameAr = name;

        public void UpdateDescription(string description) => Description = description;
    }
}