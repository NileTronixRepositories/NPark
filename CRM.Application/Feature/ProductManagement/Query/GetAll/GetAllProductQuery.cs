using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CRM.Application.Feature.ProductManagement.Query.GetAll
{
    public sealed class GetAllProductQuery : SearchParameters, IQuery<Pagination<GetAllProductQueryResponse>>
    {
    }
}