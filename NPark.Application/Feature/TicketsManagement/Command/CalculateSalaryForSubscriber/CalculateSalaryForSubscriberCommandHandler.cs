using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Feature.TicketsManagement.Command.CalculateSalary;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalaryForSubscriber
{
    public sealed class CalculateSalaryForSubscriberCommandHandler : ICommandHandler<CalculateSalaryForSubscriberCommand, CalculateSalaryCommandResponse>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;
        private readonly IGenericRepository<Ticket> _ticketRepository;

        public CalculateSalaryForSubscriberCommandHandler(
            IGenericRepository<ParkingMemberships> parkingMembershipsRepository,
            IGenericRepository<Ticket> ticketRepository)
        {
            _parkingMembershipsRepository = parkingMembershipsRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<Result<CalculateSalaryCommandResponse>> Handle(CalculateSalaryForSubscriberCommand request, CancellationToken cancellationToken)
        {
            var specCard = new GetCardSummaryByIdSpec(request.CardNumber);

            var Subscriber = await _parkingMembershipsRepository.FirstOrDefaultWithSpecAsync(specCard, cancellationToken);
            if (Subscriber == null)
            {
                return Result<CalculateSalaryCommandResponse>.
                    Fail(new Error("Card not found", "Card not found", ErrorType.NotFound));
            }
            if (DateTime.Now < Subscriber.CreatedAt || DateTime.Now >= Subscriber.EndDate)
            {
                return Result<CalculateSalaryCommandResponse>.Fail
                        (new Error("invalid card date", "invalid card date", ErrorType.NotFound));
            }
            var spec = new GetTicketByNationalIdSpec(Subscriber.NationalId);
            var ticket = await _ticketRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            if (ticket == null)
            {
                return Result<CalculateSalaryCommandResponse>.
                    Fail(new Error("Ticket not found", "Ticket not found", ErrorType.NotFound));
            }
            var response = new CalculateSalaryCommandResponse()
            {
                EnterDate = ticket.StartDate,
                IsCollectByCashier = ticket.IsCashierCollected,
                IsExitValid = true,
                TotalSalary = 0,
            };
            ticket.SetExitDate();
            await _ticketRepository.SaveChangesAsync(cancellationToken);
            return Result<CalculateSalaryCommandResponse>.Ok(response);
        }
    }
}