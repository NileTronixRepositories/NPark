using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetInactiveMembership;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.ParkingMembershipSpecification
{
    public class GetInactiveMembershipSpecification : Specification<ParkingMemberships, GetInactiveMembershipQueryResponse>
    {
        public GetInactiveMembershipSpecification()
        {
            AddCriteria(p => p.EndDate < DateTime.Now);
            Select(p => new GetInactiveMembershipQueryResponse
            {
                Id = p.Id,
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