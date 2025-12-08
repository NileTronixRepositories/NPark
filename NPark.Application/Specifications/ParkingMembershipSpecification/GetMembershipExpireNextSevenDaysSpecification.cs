using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetMembershipExpireNextSevenDays;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.ParkingMembershipSpecification
{
    public sealed class GetMembershipExpireNextSevenDaysSpecification : Specification<ParkingMemberships, GetMembershipExpireNextSevenDaysQueryResponse>
    {
        public GetMembershipExpireNextSevenDaysSpecification()
        {
            var now = DateTime.Now;
            var sevenDaysLater = now.AddDays(7);
            AddCriteria(m => m.EndDate >= now &&
                    m.EndDate <= sevenDaysLater);
            Select(m => new GetMembershipExpireNextSevenDaysQueryResponse
            {
                CardNumber = m.CardNumber,
                CreatedAt = m.CreatedAt,
                EndDate = m.EndDate,
                Name = m.Name,
                NationalId = m.NationalId,
                Phone = m.Phone,
                VehicleNumber = m.VehicleNumber
            ,
                PricingSchemeName = m.PricingScheme.Name
            });
        }
    }
}