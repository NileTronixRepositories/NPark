namespace NPark.Application.Shared.Dto
{
    /// <summary>
    /// Payload sent to dashboard when a ticket exits.
    /// </summary>
    public sealed record TicketExitedNotification
    {
        public Guid TicketId { get; init; }
        public Guid GateId { get; init; }
        public Guid UserId { get; init; }
        public DateTime ExitDate { get; init; }
    }
}