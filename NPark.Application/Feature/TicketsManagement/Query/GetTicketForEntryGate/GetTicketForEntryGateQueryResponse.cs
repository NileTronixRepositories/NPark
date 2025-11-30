namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate
{
    public sealed record GetTicketForEntryGateQueryResponse
    {
        public decimal TotalPrice { get; set; }
        public List<GetTicketInfo> TicketInfo { get; set; } = new();
    }

    public sealed record GetTicketInfo
    {
        public DateTime StartDate { get; init; }
        public string? TicketNumber { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public bool isCollectedByCashier { get; init; }
        public string? TicketInfo { get; init; } = string.Empty;
    }
}