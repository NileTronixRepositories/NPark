using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public sealed class Site : Entity<Guid>
    {
        private List<SiteProduct> _siteProducts = new List<SiteProduct>(); // <==>
        private List<Ticket> _tickets = new List<Ticket>(); // <==>
        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; } = string.Empty;
        public Guid AccountId { get; private set; }
        public Account Account { get; private set; }
        public IReadOnlyCollection<SiteProduct> SiteProducts => _siteProducts;
        public IReadOnlyCollection<Ticket> Tickets => _tickets;

        private Site()
        {
        }

        public static Site Create(string nameEn, string? nameAr) => new Site()
        {
            Id = Guid.NewGuid(),
            NameEn = nameEn,
            NameAr = nameAr
        };

        public void AddSiteProduct(SiteProduct siteProduct) => _siteProducts.Add(siteProduct);

        public void RemoveSiteProduct(SiteProduct siteProduct) => _siteProducts.Remove(siteProduct);
    }
}