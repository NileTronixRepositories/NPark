using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetActiveMembership;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.ParkingMembershipSpecification
{
    public sealed class GetActiveMembershipSpecification : Specification<ParkingMemberships, GetActiveMembershipQueryResponse>
    {
        public GetActiveMembershipSpecification()
        {
            AddCriteria(p => p.EndDate >= DateTime.Now);
            Select(p => new GetActiveMembershipQueryResponse
            {
                CardNumber = p.CardNumber,
                CreatedAt = p.CreatedAt,
                EndDate = p.EndDate,
                Name = p.Name,
                NationalId = p.NationalId,
                Phone = p.Phone,
                VehicleNumber = p.VehicleNumber,
                PricingSchemeName = p.PricingScheme.Name
            });
        }
    }
}