using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Logging;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.ColletByCachier
{
    public sealed class ColletByCachierCommandHandler : ICommandHandler<ColletByCachierCommand>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ILogger<ColletByCachierCommandHandler> _logger;

        public ColletByCachierCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
            ILogger<ColletByCachierCommandHandler> logger)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                // ---------------------------
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