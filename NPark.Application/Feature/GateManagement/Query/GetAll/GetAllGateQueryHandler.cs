using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.GateManagement.Query.GetAll
{
    public sealed class GetAllGateQueryHandler : IQueryHandler<GetAllGateQuery, IReadOnlyList<GetAllGateQueryResponse>>
    {
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;
        public GetAllGateQueryHandler(IGenericRepository<ParkingGate> parkingGateRepository)
        {
            _parkingGateRepository = parkingGateRepository;
        }
        public async Task<Result<IReadOnlyList<GetAllGateQueryResponse>>> Handle(GetAllGateQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetAllGateSpec();
            var result = await _parkingGateRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetAllGateQueryResponse>>.Ok(result);
        }
    }
}
