using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate
{
    public sealed class GetTicketForExitGateQueryHandler : IQueryHandler<GetTicketForExitGateQuery, IReadOnlyList<GetTicketForExitGateQueryResponse>>
    {
        private readonly IGenericRepository<Ticket> _repo;

        public GetTicketForExitGateQueryHandler(IGenericRepository<Ticket> repository)
        {
            _repo = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<Result<IReadOnlyList<GetTicketForExitGateQueryResponse>>> Handle(GetTicketForExitGateQuery request, CancellationToken cancellationToken)
        {
            var spec = new TicketsCreatedTodayForExitGateSpec();
            var entities = await _repo.ListWithSpecAsync(spec);
            return Result<IReadOnlyList<GetTicketForExitGateQueryResponse>>
                .Ok(entities);

        }
    }
}
