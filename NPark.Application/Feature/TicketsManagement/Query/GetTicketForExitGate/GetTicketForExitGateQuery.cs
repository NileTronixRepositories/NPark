using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate
{
    public sealed record GetTicketForExitGateQuery : IQuery<IReadOnlyList<GetTicketForExitGateQueryResponse>>
    {
    }
}
