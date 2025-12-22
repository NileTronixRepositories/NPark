using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CRM.Application.Feature.AccountManagement.Query.GetAll
{
    public sealed class GetAllAccountQuery : SearchParameters, IQuery<Pagination<GetAllAccountQueryResponse>>
    {
    }
}