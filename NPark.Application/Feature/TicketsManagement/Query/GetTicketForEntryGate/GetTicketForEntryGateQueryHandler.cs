using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate
{
    public sealed class GetTicketForEntryGateQueryHandler : IQueryHandler<GetTicketForEntryGateQuery, IReadOnlyList<GetTicketForEntryGateQueryResponse>>
    {
        private readonly IGenericRepository<Ticket> _repo;

        public GetTicketForEntryGateQueryHandler(IGenericRepository<Ticket> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<Result<IReadOnlyList<GetTicketForEntryGateQueryResponse>>> Handle(GetTicketForEntryGateQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}