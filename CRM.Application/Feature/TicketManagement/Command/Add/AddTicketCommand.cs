using BuildingBlock.Application.Abstraction;
using CRM.Domain.Enums;

namespace CRM.Application.Feature.TicketManagement.Command.Add
{
    public sealed record AddTicketCommand : ICommand
    {
        public string Description { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; } = string.Empty;
        public TicketSeverity Severity { get; init; }
        public Guid SiteId { get; init; }
        public Guid ProductId { get; init; }
    }
}