namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate
{
    public sealed record GetTicketForExitGateQueryResponse
    {
        public DateTime StartDate { get; init; }
        public Guid TicketNumber { get; init; }
        public decimal Price { get; init; }
        public bool isCollectedByCashier { get; init; }
    }
}