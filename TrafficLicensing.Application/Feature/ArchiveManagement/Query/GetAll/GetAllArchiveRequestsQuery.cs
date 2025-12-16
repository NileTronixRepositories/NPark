using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetAll
{
    public sealed class GetAllArchiveRequestsQuery : SearchParameters, IQuery<Pagination<GetAllArchiveRequestsQueryResponse>>
    {
    }
}