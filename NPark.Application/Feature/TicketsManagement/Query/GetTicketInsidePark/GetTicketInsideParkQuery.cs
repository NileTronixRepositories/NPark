using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketInsidePark
{
    public sealed record GetTicketInsideParkQuery : IQuery<IReadOnlyList<GetTicketInsideParkQueryResponse>>
    {
    }
}