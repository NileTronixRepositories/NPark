using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.ProductManagement.Query.GetProducts;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.ProductSpecification
{
    internal sealed class GetProductsSpec : Specification<Product, GetProductsResponse>
    {
        public GetProductsSpec()
        {
            Select(x => new GetProductsResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
            });
            UseNoTracking();
        }
    }
}