using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;

namespace NPark.Application.Feature.TicketsManagement.Command.SearchTicket
{
    public sealed class SearchTicketCommand : SearchParameters, ICommand<IReadOnlyList<GetTicketInfo>>
    {
    }
}