using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.CollectDailyTicket
{
    public sealed record CollectDailyTicketCommand : ICommand<CollectDailyTicketResponse>
    {
    }
}