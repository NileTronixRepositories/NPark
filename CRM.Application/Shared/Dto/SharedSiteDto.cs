using CRM.Domain.Enums;

namespace CRM.Application.Shared.Dto
{
    public sealed record SharedSiteDto
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
        public List<SharedProductDto> Products { get; init; } = new();
        public List<SharedTicketDto> Tickets { get; init; } = new();
    }

    public sealed record SharedProductDto
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
        public string? ProductImage { get; init; }
        public string? DescriptionEn { get; init; } = string.Empty;
        public string? DescriptionAr { get; init; }
    }

    public sealed record SharedTicketDto
    {
        public Guid Id { get; init; }

        public string Subject { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public TicketStatus Status { get; init; }
        public TicketSeverity Severity { get; init; }
        public List<SharedTicketAttachmentDto> Attachments { get; init; } = new();
    }
    public sealed record SharedTicketAttachmentDto
    {
        public Guid Id { get; init; }
        public string? Url { get; init; }
    }
}