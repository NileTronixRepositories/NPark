using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate
{
    public sealed class GetTicketForExitGateQueryHandler : IQueryHandler<GetTicketForExitGateQuery, GetTicketForExitGateQueryResponse>
    {
        private readonly IGenericRepository<Ticket> _repo;

        public GetTicketForExitGateQueryHandler(IGenericRepository<Ticket> repository)
        {
            _repo = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result<GetTicketForExitGateQueryResponse>> Handle(GetTicketForExitGateQuery request, CancellationToken cancellationToken)
        {
            var spec = new TicketsCreatedTodayForExitGateSpec();
            var ticketsInfo = await _repo.ListWithSpecAsync(spec);
            var result = new GetTicketForExitGateQueryResponse
            {
                TicketInfo = ticketsInfo
            };
            result.TotalPrice = result.TicketInfo.Sum(x => x.Price);
            return Result<GetTicketForExitGateQueryResponse>
                .Ok(result);
        }
    }
}