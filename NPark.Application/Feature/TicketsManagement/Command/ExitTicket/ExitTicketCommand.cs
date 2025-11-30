using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.ExitTicket
{
    public sealed record ExitTicketCommand : ICommand
    {
        public Guid TicketId { get; init; }
    }
}