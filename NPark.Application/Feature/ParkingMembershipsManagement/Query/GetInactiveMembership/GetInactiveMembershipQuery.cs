using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetInactiveMembership
{
    public sealed record GetInactiveMembershipQuery : IQuery<IReadOnlyList<GetInactiveMembershipQueryResponse>>
    {
    }
}