using BuildingBlock.Application.Abstraction;
using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Command.ActionTaken
{
    public sealed record ActionTakenOnArchiveCommand : ICommand
    {
        public Guid Id { get; init; }
        public ArchiveAction ActionTaken { get; init; }
        public string? RejectReason { get; init; }
    }
}