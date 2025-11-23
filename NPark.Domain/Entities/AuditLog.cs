using BuildingBlock.Domain.EntitiesHelper;

namespace NPark.Domain.Entities
{
    public sealed class AuditLog : Entity<Guid>
    {
        public DateTime CreatedAtUtc { get; init; }

        // WHAT
        public string EventName { get; init; } = null!;   // ex: "TicketCollected", "GateEntered", "GateExited", "UnhandledException"

        public string? EventCategory { get; init; }       // ex: "Ticket", "Gate", "Error"

        // WHO
        public Guid? UserId { get; init; }

        public Guid? GateId { get; init; }
        public string? Role { get; init; }

        // HTTP
        public string? RequestPath { get; init; }

        public string? HttpMethod { get; init; }
        public int? StatusCode { get; init; }

        // ERROR
        public bool IsSuccess { get; init; }

        public string? ErrorCode { get; init; }           // ex: "Unknown.Exception" أو Error.Code من Result
        public string? ErrorMessage { get; init; }        // Short message (مش الـ stack)

        // CORR
        public string? CorrelationId { get; init; }

        public string? TraceId { get; init; }

        // EXTRA
        public string? ExtraJson { get; init; }
    }
}