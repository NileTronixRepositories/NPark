using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.OrderPriceSchemaSpecification
{
    public class OrderPriceSchemaSpec : Specification<OrderPricingSchema>
    {
        public OrderPriceSchemaSpec()
        {
            Include(x => x.PricingScheme);
            AddOrderBy(x => x.Count);
        }
    }
}