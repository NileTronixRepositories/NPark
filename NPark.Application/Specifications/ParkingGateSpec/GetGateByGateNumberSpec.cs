using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Specifications.ParkingGateSpec
{
    public sealed class GetGateByGateNumberSpec : Specification<ParkingGate>
    {
        public GetGateByGateNumberSpec(int gateNumber, GateType gateType)
        {
            AddCriteria(x => x.GateNumber == gateNumber && x.GateType == gateType);
        }
    }
}