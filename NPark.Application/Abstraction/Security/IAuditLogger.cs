using NPark.Application.Shared.Dto;

namespace NPark.Application.Abstraction.Security
{
    public interface IAuditLogger
    {
        Task LogAsync(AuditLogEntry entry, CancellationToken ct = default);
    }
}