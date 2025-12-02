using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.ColletByCachier
{
    public sealed class ColletByCachierCommandHandler : ICommandHandler<ColletByCachierCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ILogger<ColletByCachierCommandHandler> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;

        public ColletByCachierCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
            IHttpContextAccessor contextAccessor,
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository,
            ITokenReader tokenReader,
            ILogger<ColletByCachierCommandHandler> logger)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
        }

        public async Task<Result> Handle(
            ColletByCachierCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Get ticket by ID
                // ---------------------------
                var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

                if (ticket is null)
                {
                    _logger.LogWarning(
                        "Ticket not found for cashier collection. TicketId: {TicketId}",
                        request.TicketId);

                    return Result.Fail(
                        new Error(
                            Code: "Ticket.NotFound",
                            Message: ErrorMessage.Ticket_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 2) Check if already collected by cashier
                // ---------------------------
                if (ticket.IsCashierCollected)
                {
                    _logger.LogWarning(
                        "Ticket already collected by cashier. TicketId: {TicketId}",
                        request.TicketId);

                    return Result.Fail(
                        new Error(
                            Code: "Ticket.AlreadyCollected",
                            Message: ErrorMessage.Ticket_AlreadyCollected,
                            Type: ErrorType.Conflict));
                }
                //----------------------------
                // Get Token Info
                // ---------------------------

                var tokenInfo = _contextAccessor.HttpContext?.ReadToken(_tokenReader);
                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.Role))
                    return Result.Fail(new Error(
                            Code: "Token.Missing",
                            Message: ErrorMessage.TokenInfo_Missing,
                            Type: ErrorType.Security));

                //------------------------------------
                //Get Configuration
                //------------------------------------
                var spec = new GetParkingSystemConfigurationSpecification();
                var configurationEntity = await _parkingSystemConfigurationRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
                if (configurationEntity is null)
                    return Result.Fail(new Error(
                            Code: "Configuration.NotFound",
                            Message: ErrorMessage.Configuration_NotFound,
                            Type: ErrorType.NotFound));

                //--------------------------
                //Check valid of rule for cashier collect
                //--------------------------

                if (configurationEntity.PriceType == PriceType.Enter && tokenInfo.Role == "ExitCashier")
                    return Result.Fail(new Error(
                            Code: "Invalid.Exit.Collect.Cashier",
                            Message: ErrorMessage.GateInvalidForCollectExitPricing,
                            Type: ErrorType.NotFound));

                if (configurationEntity.PriceType == PriceType.Exit && tokenInfo.Role == "EntranceCashier")
                    return Result.Fail(new Error(
                            Code: "Invalid.Enter.Collect.Cashier",
                            Message: ErrorMessage.GateInvalidForCollectEnterPricing,
                            Type: ErrorType.NotFound));

                // 3) Mark as collected by cashier
                // ---------------------------
                ticket.SetIsCashierCollected();

                await _ticketRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Ticket collected by cashier successfully. TicketId: {TicketId}",
                    request.TicketId);

                return Result.Ok();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "CollectByCashier operation was canceled. TicketId: {TicketId}",
                    request.TicketId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while collecting ticket by cashier. TicketId: {TicketId}",
                    request.TicketId);

                return Result.Fail(
                    new Error(
                        Code: "Ticket.CollectByCashier.Unexpected",
                        Message: ErrorMessage.TicketCollectByCashier_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }
    }
}