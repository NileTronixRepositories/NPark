namespace NPark.Application.Feature.TicketsManagement.Command.CollectDailyTicket
{
    public sealed record CollectDailyTicketResponse
    {
        public decimal TotalPriceCollected { get; init; }
        public int TotalTicketsCollected { get; init; }
        public List<TicketCollectDetails> TicketCollectDetails { get; init; } = new();
    }
    public sealed record TicketCollectDetails
    {
        public DateTime CollectedAt { get; init; }
        public Guid TicketNumber { get; init; }
        public decimal Price { get; init; }
    }
}