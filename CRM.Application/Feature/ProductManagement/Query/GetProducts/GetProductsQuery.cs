using BuildingBlock.Application.Abstraction;

namespace CRM.Application.Feature.ProductManagement.Query.GetProducts
{
    public sealed record GetProductsQuery : IQuery<IReadOnlyList<GetProductsResponse>>
    {
    }
}