using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate
{
    public sealed record GetTicketForExitGateQueryResponse
    {
        public decimal TotalPrice { get; set; }
        public List<GetTicketInfo> TicketInfo { get; set; } = new();
    }
}