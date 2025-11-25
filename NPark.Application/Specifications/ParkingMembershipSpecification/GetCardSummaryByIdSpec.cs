using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetSummaryById;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.ParkingMembershipSpecification
{
    public sealed class GetCardSummaryByIdSpec : Specification<ParkingMemberships, GetCardSummaryByIdQueryResponse>
    {
        public GetCardSummaryByIdSpec(string cardNumber)
        {
            AddCriteria(x => x.CardNumber == cardNumber);
            Include(x => x.PricingScheme);
            Select(x => new GetCardSummaryByIdQueryResponse
            {
                CardNumber = x.CardNumber,
                CreatedAt = x.CreatedAt,
                EndDate = x.EndDate,
                Name = x.Name,
                NationalId = x.NationalId,
                Phone = x.Phone,
                VehicleNumber = x.VehicleNumber
                ,
                Price = x.PricingScheme.Salary
            });
            UseNoTracking();
        }
    }
}