namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate
{
    public sealed record GetTicketForEntryGateQueryResponse
    {
        public DateTime StartDate { get; init; }
        public Guid TicketNumber { get; init; }
        public decimal Price { get; init; }
        public bool isCollectedByCashier { get; init; }
    }
}