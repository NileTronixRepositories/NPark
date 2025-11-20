using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.ParkingSystemConfigurationManagement.Command.Update;
using NPark.Application.Feature.ParkingSystemConfigurationManagement.Query.GetSystemConfiguration;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Specifications.ParkingSystemConfigurationSpec
{
    public sealed class GetParkingSystemConfigurationSpecification : Specification<ParkingSystemConfiguration, GetSystemConfigurationQueryResponse>
    {
        public GetParkingSystemConfigurationSpecification()
        {
            AddCriteria(x => x.Id == 1);
            Include(x => x.PricingScheme);
            Include(x => x.ParkingGates);
            Select(x => new GetSystemConfigurationQueryResponse
            {
                AllowedParkingSlots = x.AllowedParkingSlots,
                EntryGatesCount = x.EntryGatesCount,
                ExitGatesCount = x.ExitGatesCount,
                PriceType = x.PriceType,
                VehiclePassengerData = x.VehiclePassengerData,
                PrintType = x.PrintType,
                DateTimeFlag = x.DateTimeFlag,
                TicketIdFlag = x.TicketIdFlag,
                FeesFlag = x.FeesFlag,
                gracePeriodMinutes = x.GracePeriod == null ? null : (int)x.GracePeriod.Value.TotalMinutes,
                PricingSchemaId = x.PricingSchemaId,
                PricingSchemaName = (x.PricingScheme == null) ? string.Empty : x.PricingScheme.Name,
                ExitGates = x.ParkingGates.Where(x => x.GateType == GateType.Exit)
                .Select(x => new GateInfo
                {
                    GateNumber = x.GateNumber,
                    LprIp = x.LprIp,
                    PcIp = x.PcIp,
                }).ToList(),
                EntryGates = x.ParkingGates.Where(x => x.GateType == GateType.Entrance)
                .Select(x => new GateInfo
                {
                    GateNumber = x.GateNumber,
                    LprIp = x.LprIp,
                    PcIp = x.PcIp,
                }).ToList()
            });
        }
    }
}