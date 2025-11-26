using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed record AddTicketCommand : ICommand<AddTicketCommandResponse>
    {
        // Vehicle number is optional (for non-subscribers).
        public string? VehicleNumber { get; init; }

        // True => Subscriber path, False => Normal ticket.
        public bool IsSubscriber { get; init; }

        // CardNumber is only required when IsSubscriber == true.
        public string? CardNumber { get; init; }
    }
}