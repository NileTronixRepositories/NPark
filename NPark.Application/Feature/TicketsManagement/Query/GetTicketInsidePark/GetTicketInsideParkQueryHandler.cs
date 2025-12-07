using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketInsidePark
{
    public sealed class GetTicketInsideParkQueryHandler : IQueryHandler<GetTicketInsideParkQuery, IReadOnlyList<GetTicketInsideParkQueryResponse>>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;

        public GetTicketInsideParkQueryHandler(IGenericRepository<Ticket> ticketRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        }

        public async Task<Result<IReadOnlyList<GetTicketInsideParkQueryResponse>>> Handle(GetTicketInsideParkQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetTicketInsideParkSpecification();
            var response = await _ticketRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetTicketInsideParkQueryResponse>>.Ok(response);
        }
    }
}