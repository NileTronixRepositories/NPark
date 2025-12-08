using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetMembershipExpireNextSevenDays
{
    public sealed record GetMembershipExpireNextSevenDaysQuery : IQuery<IReadOnlyList<GetMembershipExpireNextSevenDaysQueryResponse>>
    {
    }
}