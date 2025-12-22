using BuildingBlock.Domain.EntitiesHelper;
using BuildingBlock.Domain.Primitive;

namespace CRM.Domain.Entities
{
    public sealed class Account : AggregateRoot<Guid>, ISoftDeleteEntity
    {
        private List<Site> _sites = new();
        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool IsLoginBefore { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public DateTime? RestoredOnUtc { get; set; }
        public Guid RoleId { get; private set; }
        public Role Role { get; private set; }

        public IReadOnlyCollection<Site> Sites => _sites;

        private Account()
        { }

        public static Account Create(
            string nameEn,
            string? nameAr,
            string email,
            string password
            )
        {
            return new Account()
            {
                NameEn = nameEn,
                NameAr = nameAr,
                Email = email,
                Password = password,
                IsLoginBefore = false,
                IsDeleted = false
            };
        }

        public void AddSite(Site site) => _sites.Add(site);

        public void AddSites(IEnumerable<Site> sites) => _sites.AddRange(sites);

        public void RemoveSite(Site site) => _sites.Remove(site);

        public void AssignRole(Guid roleId) => RoleId = roleId;
    }
}