using CRM.Domain.Enums;

namespace CRM.Application.Feature.TicketManagement.Query.GetAllSuperAdmin
{
    public sealed record GetAllSuperAdminQueryResponse
    {
        public Guid Id { get; init; }
        public string Description { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; } = string.Empty;
        public TicketStatus Status { get; init; }
        public TicketSeverity Severity { get; init; }
        public string SiteNameEn { get; init; } = string.Empty;
        public string? SiteNameAr { get; init; } = string.Empty;
        public string AccountNameEn { get; init; } = string.Empty;
        public string? AccountNameAr { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public bool IsInProducts { get; init; }
        public bool IsInSupport { get; init; }
    }
}