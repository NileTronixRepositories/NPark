using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public sealed class Product : Entity<Guid>
    {
        private List<SiteProduct> _siteProducts = new List<SiteProduct>(); // <==>
        private List<Ticket> _tickets = new List<Ticket>(); // <==>

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string? DescriptionEn { get; private set; }
        public string? DescriptionAr { get; private set; }
        public string? ImagePath { get; private set; }

        public IReadOnlyCollection<SiteProduct> SiteProducts => _siteProducts;
        public IReadOnlyCollection<Ticket> Tickets => _tickets;

        private Product()
        {
        }

        public static Product Create(string nameEn, string? nameAr, string? descriptionEn, string? descriptionAr, string? imagePath) => new Product()
        {
            NameEn = nameEn,
            NameAr = nameAr,
            DescriptionEn = descriptionEn,
            DescriptionAr = descriptionAr,
            ImagePath = imagePath
        };
    }
}