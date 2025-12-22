using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public sealed class Site : Entity<Guid>
    {
        public string NameEn { get; private set; } = string.Empty;
        public string NameAr { get; private set; } = string.Empty;
        public Guid AccountId { get; private set; }
        public Account Account { get; private set; }

        private Site()
        {
        }

        public static Site Create(string nameEn, string nameAr) => new Site()
        {
            Id = Guid.NewGuid(),
            NameEn = nameEn,
            NameAr = nameAr
        };
    }
}