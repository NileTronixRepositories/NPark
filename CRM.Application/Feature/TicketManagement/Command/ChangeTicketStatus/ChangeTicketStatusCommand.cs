using BuildingBlock.Application.Abstraction;
using CRM.Domain.Enums;

namespace CRM.Application.Feature.TicketManagement.Command.ChangeTicketStatus
{
    public sealed record ChangeTicketStatusCommand : ICommand
    {
        public Guid Id { get; init; }
        public string? Description { get; init; }
        public TicketStatus Status { get; init; }
        public TicketSeverity Severity { get; init; }
    }
}