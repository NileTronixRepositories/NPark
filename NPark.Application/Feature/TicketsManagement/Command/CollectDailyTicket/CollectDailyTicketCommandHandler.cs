using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;
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
        private readonly ILogger<CollectDailyTicketCommandHandler> _logger;

        public CollectDailyTicketCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
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
        }

        public async Task<Result<CollectDailyTicketResponse>> Handle(
            CollectDailyTicketCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Read token info (GateId + UserId)
                // ---------------------------
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

                // ---------------------------
                // 2) Get today's tickets for this gate
                // ---------------------------
                var spec = new TotalTicketsForTodaySpec(gateId);
                var tickets = await _ticketRepository
                    .ListWithSpecAsync(spec, cancellationToken);

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

                // ---------------------------
                // 3) Mark tickets as collected
                // ---------------------------
                foreach (var ticket in ticketList)
                {
                    ticket.SetCollected(userId);
                }

                // نستخدم وقت واحد للتحصيل لكل التفاصيل (consistent timestamp)
                var collectedAt = DateTime.Now; // أو DateTime.UtcNow لو النظام كله UTC

                var response = new CollectDailyTicketResponse
                {
                    TotalTicketsCollected = ticketList.Count,
                    TotalPriceCollected = ticketList.Sum(x => x.Price),
                    TicketCollectDetails = ticketList
                        .Select(x => new TicketCollectDetails
                        {
                            CollectedAt = collectedAt,
                            TicketNumber = x.Id,
                            Price = x.Price
                        })
                        .ToList()
                };

                // ---------------------------
                // 4) Save changes
                // ---------------------------
                await _ticketRepository.SaveChangesAsync(cancellationToken);

                // ---------------------------
                // 5) Audit log (Safe)
                // ---------------------------
                await SafeAuditAsync(response, userId, cancellationToken);

                // ---------------------------
                // 6) Return success
                // ---------------------------
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

        // --------------------------------------------------------
        // Safe audit logging: لا يوقع الريكوست لو حصل خطأ في اللوج
        // --------------------------------------------------------
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
                // مهم: اللوج لو وقع ما يبوّظش الريكوست
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for CollectDailyTicket. UserId: {UserId}",
                    userId);
            }
        }
    }
}