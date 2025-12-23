using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public sealed class SiteProduct : Entity<Guid>
    {
        public Guid SiteId { get; private set; }
        public Guid ProductId { get; private set; }
        public Site Site { get; private set; }
        public Product Product { get; private set; }
        public DateTime SupportEndDate { get; private set; }

        private SiteProduct()
        {
        }

        public static SiteProduct Create(Guid siteId, Guid productId, DateTime supportEndDate) => new SiteProduct()
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            ProductId = productId,
            SupportEndDate = supportEndDate
        };
    }
}