using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate
{
    public sealed class GetTicketForEntryGateQueryHandler : IQueryHandler<GetTicketForEntryGateQuery, IReadOnlyList<GetTicketForEntryGateQueryResponse>>
    {
        private readonly IGenericRepository<Ticket> _repo;

        public GetTicketForEntryGateQueryHandler(IGenericRepository<Ticket> repository)
        {
            _repo = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result<IReadOnlyList<GetTicketForEntryGateQueryResponse>>> Handle(GetTicketForEntryGateQuery request, CancellationToken cancellationToken)
        {
            var spec = new TicketsCreatedTodayForEntryGateSpec();
            var entities = await _repo.ListWithSpecAsync(spec);
            return Result<IReadOnlyList<GetTicketForEntryGateQueryResponse>>
                .Ok(entities);
        }
    }
}