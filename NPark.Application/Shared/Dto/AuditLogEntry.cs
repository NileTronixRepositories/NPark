namespace NPark.Application.Shared.Dto
{
    public sealed record AuditLogEntry(
     string EventName,
     string? EventCategory = null,
     bool IsSuccess = true,
     int? StatusCode = null,
     string? ErrorCode = null,
     string? ErrorMessage = null,
     Guid? UserId = null,
     Guid? GateId = null,
     string? Role = null,
     string? RequestPath = null,
     string? HttpMethod = null,
     string? CorrelationId = null,
     string? TraceId = null,
     object? Extra = null
 );
}