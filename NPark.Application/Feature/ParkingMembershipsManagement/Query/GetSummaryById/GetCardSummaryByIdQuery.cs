using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetSummaryById
{
    public sealed record GetCardSummaryByIdQuery : IQuery<GetCardSummaryByIdQueryResponse>
    {
        public string CardNumber { get; init; } = string.Empty;
    }
}