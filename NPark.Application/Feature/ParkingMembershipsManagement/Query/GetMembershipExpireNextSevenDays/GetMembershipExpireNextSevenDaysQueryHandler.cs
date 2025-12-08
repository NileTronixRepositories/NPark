using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetMembershipExpireNextSevenDays
{
    public sealed class GetMembershipExpireNextSevenDaysQueryHandler : IQueryHandler<GetMembershipExpireNextSevenDaysQuery, IReadOnlyList<GetMembershipExpireNextSevenDaysQueryResponse>>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;

        public GetMembershipExpireNextSevenDaysQueryHandler(IGenericRepository<ParkingMemberships> parkingMembershipsRepository)
        {
            _parkingMembershipsRepository = parkingMembershipsRepository ?? throw new ArgumentNullException(nameof(parkingMembershipsRepository));
        }

        public async Task<Result<IReadOnlyList<GetMembershipExpireNextSevenDaysQueryResponse>>> Handle(GetMembershipExpireNextSevenDaysQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetMembershipExpireNextSevenDaysSpecification();
            var response = await _parkingMembershipsRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetMembershipExpireNextSevenDaysQueryResponse>>.Ok(response);
        }
    }
}