using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetSummaryById
{
    public sealed class GetCardSummaryByIdQueryHandler : IQueryHandler<GetCardSummaryByIdQuery, GetCardSummaryByIdQueryResponse>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;

        public GetCardSummaryByIdQueryHandler(IGenericRepository<ParkingMemberships> parkingMembershipsRepository)
        {
            _parkingMembershipsRepository = parkingMembershipsRepository ?? throw new ArgumentNullException(nameof(parkingMembershipsRepository));
        }

        public async Task<Result<GetCardSummaryByIdQueryResponse>> Handle(GetCardSummaryByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetCardSummaryByIdSpec(request.CardNumber);
            var result = await _parkingMembershipsRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            if (result == null)
            {
                return Result<GetCardSummaryByIdQueryResponse>.
                    Fail(new Error("Card not found", "Card not found", ErrorType.NotFound));
            }

            return Result<GetCardSummaryByIdQueryResponse>.Ok(result);
        }
    }
}