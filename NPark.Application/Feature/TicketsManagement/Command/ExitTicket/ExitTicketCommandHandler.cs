using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.ExitTicket
{
    public sealed class ExitTicketCommandHandler : ICommandHandler<ExitTicketCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly ILogger<ExitTicketCommandHandler> _logger;

        public ExitTicketCommandHandler(IGenericRepository<Ticket> ticketRepository,
            IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader,
            IRealtimeNotifier realtimeNotifier, ILogger<ExitTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenReader = tokenReader;
            _realtimeNotifier = realtimeNotifier;
            _logger = logger;
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
            await NotifyDashboardTicketExitedAsync(ticketEntity, gateId, userId, cancellationToken);
            return Result.Ok();
        }

        private async Task NotifyDashboardTicketExitedAsync(
            Ticket ticketEntity,
            Guid gateId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                var payload = new TicketExitedNotification
                {
                    TicketId = ticketEntity.Id,
                    GateId = gateId,
                    UserId = userId,
                    // لو Ticket عنده EndDate، استخدمها، وإلا خذ Now
                    ExitDate = ticketEntity.EndDate ?? DateTime.Now
                };

                // channel name: "tickets:exited"
                await _realtimeNotifier.PublishAsync("tickets:exited", payload, cancellationToken);
            }
            catch (Exception ex)
            {
                // ما نبوّظش عملية الـ Exit لو SignalR وقع
                _logger.LogError(ex,
                    "Failed to send realtime exit notification for ticket {TicketId}",
                    ticketEntity.Id);
            }
        }
    }
}