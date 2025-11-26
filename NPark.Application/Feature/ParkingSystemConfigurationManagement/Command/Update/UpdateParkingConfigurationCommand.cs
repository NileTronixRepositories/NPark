using BuildingBlock.Application.Abstraction;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.ParkingSystemConfigurationManagement.Command.Update
{
    public class UpdateParkingConfigurationCommand : ICommand
    {
        public int EntryGatesCount { get; set; }
        public int ExitGatesCount { get; set; }
        public int? AllowedParkingSlots { get; set; }

        public PriceType PriceType { get; set; }
        public int? gracePeriodMinutes { get; set; }
        public VehiclePassengerData VehiclePassengerData { get; set; }
        public PrintType PrintType { get; set; }

        public bool DateTimeFlag { get; set; }
        public bool TicketIdFlag { get; set; }
        public bool FeesFlag { get; set; }
        public Guid? PricingSchemaId { get; set; }
        public List<GateInfo> EntryGates { get; set; } = new();
        public List<GateInfo> ExitGates { get; set; } = new();
    }

    public record GateInfo
    {
        public Guid? GateId { get; set; }
        public int GateNumber { get; set; }
        public string? LprIp { get; set; } = string.Empty;
        public string? PcIp { get; set; } = string.Empty;
    }
}