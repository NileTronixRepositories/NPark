namespace NPark.Application.Shared.Dto
{
    public sealed record TicketAddedNotification
    {
        public Guid TicketId { get; init; }
        public Guid GateId { get; init; }
        public DateTime StartDate { get; init; }
        public decimal Price { get; init; }
        public bool IsSubscriber { get; init; }
        public string? VehicleNumber { get; init; }
    }
}