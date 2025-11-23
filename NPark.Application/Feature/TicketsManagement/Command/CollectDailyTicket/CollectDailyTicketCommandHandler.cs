using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Command.CollectDailyTicket
{
    public sealed class CollectDailyTicketCommandHandler : ICommandHandler<CollectDailyTicketCommand, CollectDailyTicketResponse>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CollectDailyTicketCommandHandler
            (
            IGenericRepository<Ticket> ticketRepository,
            ITokenReader tokenReader,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Result<CollectDailyTicketResponse>> Handle(CollectDailyTicketCommand request, CancellationToken cancellationToken)
        {
            var tokenInfo = _httpContextAccessor!.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
            {
                return Result<CollectDailyTicketResponse>.Fail(new Error("GateId not found", "GateId not found", ErrorType.NotFound));
            }
            var spec = new TotalTicketsForTodaySpec();
            var tickets = await _ticketRepository.ListWithSpecAsync(spec, cancellationToken);

            if (tickets.Count() <= 0)
                Result.Fail(new Error("No Tickets to collect", "No Tickets to collect", ErrorType.Conflict));

            foreach (var ticket in tickets)
            {
                ticket.SetCollected(tokenInfo.UserId.Value);
            }
            var response = new CollectDailyTicketResponse
            {
                TotalTicketsCollected = tickets.Count(),
                TotalPriceCollected = tickets.Select(x => x.Price).Sum(),
                TicketCollectDetails = tickets.Select(x => new TicketCollectDetails
                { CollectedAt = DateTime.Now, Price = x.Price, TicketNumber = x.Id }).ToList()
            };

            await _ticketRepository.SaveChangesAsync(cancellationToken);

            return Result<CollectDailyTicketResponse>.Ok(response);
        }
    }
}