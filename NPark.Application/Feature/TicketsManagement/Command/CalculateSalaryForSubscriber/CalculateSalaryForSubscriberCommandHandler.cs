using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Logging;
using NPark.Application.Feature.TicketsManagement.Command.CalculateSalary;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalaryForSubscriber
{
    public sealed class CalculateSalaryForSubscriberCommandHandler
         : ICommandHandler<CalculateSalaryForSubscriberCommand, CalculateSalaryCommandResponse>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ILogger<CalculateSalaryForSubscriberCommandHandler> _logger;

        public CalculateSalaryForSubscriberCommandHandler(
            IGenericRepository<ParkingMemberships> parkingMembershipsRepository,
            IGenericRepository<Ticket> ticketRepository,
            ILogger<CalculateSalaryForSubscriberCommandHandler> logger)
        {
            _parkingMembershipsRepository = parkingMembershipsRepository
                ?? throw new ArgumentNullException(nameof(parkingMembershipsRepository));
            _ticketRepository = ticketRepository
                ?? throw new ArgumentNullException(nameof(ticketRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<CalculateSalaryCommandResponse>> Handle(
            CalculateSalaryForSubscriberCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Get subscriber by card number
                // ---------------------------
                var cardSpec = new GetCardSummaryByIdSpec(request.CardNumber);
                var subscriber = await _parkingMembershipsRepository
                    .FirstOrDefaultWithSpecAsync(cardSpec, cancellationToken);

                if (subscriber is null)
                {
                    _logger.LogWarning(
                        "Subscriber card not found. CardNumber: {CardNumber}",
                        request.CardNumber);

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "Subscriber.Card.NotFound",
                            Message: ErrorMessage.SubscriberCard_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 2) Validate card date (active period)
                // ---------------------------
                var now = DateTime.Now;

                if (now < subscriber.CreatedAt || now >= subscriber.EndDate)
                {
                    _logger.LogWarning(
                        "Invalid subscriber card date. CardNumber: {CardNumber}, Now: {Now}, Start: {Start}, End: {End}",
                        request.CardNumber, now, subscriber.CreatedAt, subscriber.EndDate);

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "Subscriber.Card.InvalidDate",
                            Message: ErrorMessage.Subscriber_Card_InvalidDate,
                            Type: ErrorType.Validation));
                }

                // ---------------------------
                // 3) Get ticket by subscriber national ID
                // ---------------------------
                var ticketSpec = new GetTicketByNationalIdSpec(subscriber.NationalId);
                var ticket = await _ticketRepository
                    .FirstOrDefaultWithSpecAsync(ticketSpec, cancellationToken);

                if (ticket is null)
                {
                    _logger.LogWarning(
                        "Ticket not found for subscriber. NationalId: {NationalId}",
                        subscriber.NationalId);

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "Ticket.NotFound",
                            Message: ErrorMessage.Ticket_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 4) Build response (subscriber = 0 total salary)
                // ---------------------------
                var response = new CalculateSalaryCommandResponse
                {
                    EnterDate = ticket.StartDate,
                    IsCollectByCashier = ticket.IsCashierCollected,
                    IsExitValid = true,
                    TotalSalary = 0m,
                    TicketId = ticket.Id
                };

                return Result<CalculateSalaryCommandResponse>.Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "CalculateSalaryForSubscriber operation was canceled. CardNumber: {CardNumber}",
                    request.CardNumber);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while calculating salary for subscriber. CardNumber: {CardNumber}",
                    request.CardNumber);

                return Result<CalculateSalaryCommandResponse>.Fail(
                    new Error(
                        Code: "Salary.Subscriber.Unexpected",
                        Message: ErrorMessage.SalaryCalculate_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }
    }
}