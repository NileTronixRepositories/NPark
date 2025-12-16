using BuildingBlock.Application.Abstraction;
using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Command.Add
{
    public sealed record AddArchiveRequestCommand : ICommand
    {
        public string PlateNumber { get; set; } = string.Empty;

        public ArchiveType Action { get; set; }
        public string? Note { get; set; } = string.Empty;
    }
}