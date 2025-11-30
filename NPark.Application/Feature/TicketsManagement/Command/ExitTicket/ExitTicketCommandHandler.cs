using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction.Security;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.ExitTicket
{
    public sealed class ExitTicketCommandHandler : ICommandHandler<ExitTicketCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExitTicketCommandHandler(IGenericRepository<Ticket> ticketRepository,
            IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader)
        {
            _ticketRepository = ticketRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenReader = tokenReader;
        }

        public async Task<Result> Handle(ExitTicketCommand request, CancellationToken cancellationToken)
        {
            // ---------------------------
            // 1) Read token info (GateId + UserId)
            // ---------------------------
            var tokenInfo = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);

            if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
            {
                return Result.Fail(
                    new Error(
                        Code: "Token.GateOrUser.NotFound",
                        Message: ErrorMessage.TokenInfo_Missing,
                        Type: ErrorType.NotFound));
            }

            var gateId = tokenInfo.GateId.Value;
            var userId = tokenInfo.UserId.Value;
            var ticketEntity = await _ticketRepository.GetByIdAsync(request.TicketId);

            if (ticketEntity is null)
            {
                return Result.Fail(
                    new Error(
                        Code: "Ticket.NotFound",
                        Message: ErrorMessage.Ticket_NotFound,
                        Type: ErrorType.NotFound));
            }
            ticketEntity.SetExitGate(gateId);
            ticketEntity.SetExitDate();

            await _ticketRepository.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}