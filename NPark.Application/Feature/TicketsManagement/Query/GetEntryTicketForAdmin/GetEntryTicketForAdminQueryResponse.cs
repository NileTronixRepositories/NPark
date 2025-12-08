namespace NPark.Application.Feature.TicketsManagement.Query.GetEntryTicketForAdmin
{
    public class GetEntryTicketForAdminQueryResponse
    {
        public Guid Id { get; init; }
        public DateTime StartDate { get; init; }
        public string? TicketNumber { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public bool isCollectedByCashier { get; init; }
        public string? TicketInfo { get; init; } = string.Empty;
    }
}