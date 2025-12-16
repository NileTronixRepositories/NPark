using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetToday
{
    public sealed class GetArchiveTodayQuery : SearchParameters, IQuery<Pagination<GetArchiveTodayQueryResponse>>
    {
    }
}