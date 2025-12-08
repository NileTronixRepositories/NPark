using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetInactiveMembership
{
    public sealed class GetInactiveMembershipQueryHandler : IQueryHandler<GetInactiveMembershipQuery, IReadOnlyList<GetInactiveMembershipQueryResponse>>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipRepo;

        public GetInactiveMembershipQueryHandler(IGenericRepository<ParkingMemberships> parkingMembershipRepository)
        {
            _parkingMembershipRepo = parkingMembershipRepository ?? throw new ArgumentNullException(nameof(parkingMembershipRepository));
        }

        public async Task<Result<IReadOnlyList<GetInactiveMembershipQueryResponse>>> Handle(GetInactiveMembershipQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetInactiveMembershipSpecification();
            var response = await _parkingMembershipRepo.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetInactiveMembershipQueryResponse>>.Ok(response);
        }
    }
}