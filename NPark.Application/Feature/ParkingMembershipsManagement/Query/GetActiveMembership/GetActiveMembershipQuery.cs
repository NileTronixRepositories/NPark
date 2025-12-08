using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetActiveMembership
{
    public sealed record GetActiveMembershipQuery : IQuery<IReadOnlyList<GetActiveMembershipQueryResponse>>
    {
    }
}