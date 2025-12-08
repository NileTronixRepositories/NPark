using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetActiveMembership
{
    public sealed record class GetActiveMembershipQueryHandler : IQueryHandler<GetActiveMembershipQuery, IReadOnlyList<GetActiveMembershipQueryResponse>>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;
        public GetActiveMembershipQueryHandler(IGenericRepository<ParkingMemberships> parkingMembershipsRepository)
        {
            _parkingMembershipsRepository = parkingMembershipsRepository;
        }
        public async Task<Result<IReadOnlyList<GetActiveMembershipQueryResponse>>> Handle(GetActiveMembershipQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetActiveMembershipSpecification();
            var result = await _parkingMembershipsRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetActiveMembershipQueryResponse>>.Ok(result);
        }
    }
}