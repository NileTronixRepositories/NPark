using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.ExitTicket
{
    public sealed class ExitTicketCommandHandler : ICommandHandler<ExitTicketCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ITokenReader _tokenReader;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly ILogger<ExitTicketCommandHandler> _logger;

        public ExitTicketCommandHandler(IGenericRepository<Ticket> ticketRepository,
            IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader,
            IRealtimeNotifier realTimeNotifier, ILogger<ExitTicketCommandHandler> logger, IGenericRepository<ParkingGate> parkingGateRepository)
        {
            _ticketRepository = ticketRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenReader = tokenReader;
            _realtimeNotifier = realTimeNotifier;
            _logger = logger;
            _parkingGateRepository = parkingGateRepository;
        }

        public async Task<Result> Handle(ExitTicketCommand request, CancellationToken cancellationToken)
        {
            // ----------------------------------------
            // 1) Read token info (GateId + UserId)
            // ----------------------------------------
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

            var gateInfo = await _parkingGateRepository.GetByIdAsync(tokenInfo.GateId.Value, cancellationToken);

            if (gateInfo is null)
            {
                _logger.LogWarning("Gate info not found while adding ticket.");
                return Result.Fail(
                    new Error(
                        Code: "Gate.NotFound",
                        Message: ErrorMessage.GateNotFound,
                        Type: ErrorType.NotFound));
            }

            if (ticketEntity is null)
            {
                return Result.Fail(
                    new Error(
                        Code: "Ticket.NotFound",
                        Message: ErrorMessage.Ticket_NotFound,
                        Type: ErrorType.NotFound));
            }
            if (ticketEntity.EndDate.HasValue)
            {
                return Result.Fail(
                    new Error(
                        Code: "Ticket.AlreadyExited",
                        Message: ErrorMessage.TicketAlreadyExited,
                        Type: ErrorType.Domain));
            }
            ticketEntity.SetExitGate(gateId);
            ticketEntity.SetExitDate();

            await _ticketRepository.SaveChangesAsync(cancellationToken);
            await NotifyDashboardTicketExitedAsync(ticketEntity, gateId, userId, gateInfo, cancellationToken);
            return Result.Ok();
        }

        private async Task NotifyDashboardTicketExitedAsync(
            Ticket ticketEntity,
            Guid gateId,
            Guid userId,
            ParkingGate x,
            CancellationToken cancellationToken)
        {
            try
            {
                var gateNumber = x.GateType == GateType.Entrance
    ? $"{ErrorMessage.EntranceGate} {x.GateNumber}"
    : $"{ErrorMessage.ExitGate} {x.GateNumber} ";
                var payload = new TicketExitedNotification
                {
                    TicketId = ticketEntity.Id,
                    GateId = gateId,
                    UserId = userId,
                    ExitDate = ticketEntity.EndDate ?? DateTime.Now,
                    GateNumber = gateNumber
                };

                // channel name: "tickets:exited"
                await _realtimeNotifier.NotifyTicketExitedAsync(payload
              );
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