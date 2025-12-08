namespace NPark.Application.Feature.TicketsManagement.Query.GetExitTicketForAdmin
{
    public sealed record GetExitTicketForAdminQueryResponse
    {
        public Guid Id { get; init; }
        public DateTime StartDate { get; init; }
        public string? TicketNumber { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public bool isCollectedByCashier { get; init; }
        public string? TicketInfo { get; init; } = string.Empty;
    }
}