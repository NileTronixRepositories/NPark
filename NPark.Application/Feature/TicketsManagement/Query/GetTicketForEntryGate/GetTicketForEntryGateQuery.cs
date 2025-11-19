using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate
{
    public sealed record GetTicketForEntryGateQuery : IQuery<IReadOnlyList<GetTicketForEntryGateQueryResponse>>
    {
    }
}