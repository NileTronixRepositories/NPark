using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.CollectDailyTicket
{
    public sealed class CollectDailyTicketCommandHandler
        : ICommandHandler<CollectDailyTicketCommand, CollectDailyTicketResponse>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogger _auditLogger;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;
        private readonly ILogger<CollectDailyTicketCommandHandler> _logger;
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;

        public CollectDailyTicketCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository,
            IGenericRepository<ParkingGate> parkingGateRepository,
            ITokenReader tokenReader,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogger auditLogger,
            ILogger<CollectDailyTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parkingGateRepository = parkingGateRepository ?? throw new ArgumentNullException(nameof(parkingGateRepository));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
        }

        public async Task<Result<CollectDailyTicketResponse>> Handle(
            CollectDailyTicketCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var tokenInfo = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);

                if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
                {
                    _logger.LogWarning(
                        "Token info is missing GateId/UserId while collecting daily tickets.");

                    return Result<CollectDailyTicketResponse>.Fail(
                        new Error(
                            Code: "Token.GateOrUser.NotFound",
                            Message: ErrorMessage.TokenInfo_Missing,
                            Type: ErrorType.NotFound));
                }

                var gateId = tokenInfo.GateId.Value;
                var userId = tokenInfo.UserId.Value;

                var gate = await _parkingGateRepository.GetByIdAsync(gateId, cancellationToken);
                if (gate is null)
                {
                    return Result<CollectDailyTicketResponse>.Fail(
                        new Error(
                            Code: "Gate.NotFound",
                            Message: ErrorMessage.GateNotFound,
                            Type: ErrorType.NotFound));
                }

                var confSpec = new GetParkingSystemConfigurationSpecification();
                var configurationEntity = await _parkingSystemConfigurationRepository
                    .FirstOrDefaultWithSpecAsync(confSpec, cancellationToken);

                if (configurationEntity is null)
                {
                    return Result<CollectDailyTicketResponse>.Fail(
                        new Error(
                            Code: "Configuration.NotFound",
                            Message: ErrorMessage.Configuration_NotFound,
                            Type: ErrorType.NotFound));
                }

                if (configurationEntity.PriceType == PriceType.Enter && gate.GateType == GateType.Exit)
                {
                    return Result<CollectDailyTicketResponse>.Fail(new Error(
                        Code: "Gate.InvalidForCollect.EnterPricing",
                        Message: ErrorMessage.GateInvalidForCollectExitPricing,
                        Type: ErrorType.Conflict));
                }

                if (configurationEntity.PriceType == PriceType.Exit && gate.GateType == GateType.Entrance)
                {
                    return Result<CollectDailyTicketResponse>.Fail(new Error(
                        Code: "Gate.InvalidForCollect.ExitPricing",
                        Message: ErrorMessage.GateInvalidForCollectEnterPricing,
                        Type: ErrorType.Conflict));
                }

                Specification<Ticket> spec = configurationEntity.PriceType == PriceType.Enter
            ? new TotalTicketsForTodaySpec(gateId)
            : new TotalTicketsForTodayForExitSpec(gateId);

                var tickets = await _ticketRepository.ListWithSpecAsync(spec, cancellationToken);
                var ticketList = tickets?.ToList() ?? new List<Ticket>();

                if (ticketList.Count == 0)
                {
                    _logger.LogWarning(
                        "No tickets found to collect for gate {GateId}.", gateId);

                    return Result<CollectDailyTicketResponse>.Fail(
                        new Error(
                            Code: "Tickets.NoneToCollect",
                            Message: ErrorMessage.Tickets_NoneToCollect,
                            Type: ErrorType.Conflict));
                }

                foreach (var ticket in ticketList)
                {
                    ticket.SetCollected(userId);
                }

                var fallbackCollectedAt = DateTime.Now;

                var response = new CollectDailyTicketResponse
                {
                    TotalTicketsCollected = ticketList.Count,
                    TotalPriceCollected = ticketList.Sum(x => x.Price),
                    TicketCollectDetails = ticketList
                        .Select(x => new TicketCollectDetails
                        {
                            CollectedAt = x.CollectedDate ?? fallbackCollectedAt,
                            TicketNumber = x.Id,
                            Price = x.Price
                        })
                        .ToList()
                };

                await _ticketRepository.SaveChangesAsync(cancellationToken);

                await SafeAuditAsync(response, userId, cancellationToken);

                return Result<CollectDailyTicketResponse>.Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("CollectDailyTicket operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while collecting daily tickets.");
                return Result<CollectDailyTicketResponse>.Fail(
                    new Error(
                        Code: "Tickets.CollectDaily.Unexpected",
                        Message: ErrorMessage.TicketAdd_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        private async Task SafeAuditAsync(
            CollectDailyTicketResponse response,
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "CollectDailyTicketSucceeded",
                        EventCategory: "TicketManagement",
                        IsSuccess: true,
                        StatusCode: StatusCodes.Status200OK,
                        UserId: userId,
                        GateId: null,
                        Extra: new
                        {
                            response.TotalTicketsCollected,
                            response.TotalPriceCollected,
                            TicketDetailsCount = response.TicketCollectDetails.Count,
                            CollectedByUserId = userId
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for CollectDailyTicket. UserId: {UserId}",
                    userId);
            }
        }
    }
}