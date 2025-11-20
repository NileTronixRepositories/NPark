using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.ParkingGateSpec
{
    public sealed class GetGateByGateNumberSpec : Specification<ParkingGate>
    {
        public GetGateByGateNumberSpec(int gateNumber)
        {
            AddCriteria(x => x.GateNumber == gateNumber);
        }
    }
}