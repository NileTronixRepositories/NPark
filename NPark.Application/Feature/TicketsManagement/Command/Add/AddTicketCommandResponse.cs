namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed record AddTicketCommandResponse
    {
        public byte[] QrCode { get; init; } = Array.Empty<byte>();
        public Guid TicketId { get; init; }
        public DateTime CreatedAt { get; init; }
        public decimal Price { get; init; }
    }
}