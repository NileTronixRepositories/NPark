using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.GateManagement.Query.GetAll;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Specifications.ParkingGateSpec
{
    public class GetAllGateSpec : Specification<ParkingGate, GetAllGateQueryResponse>
    {
        public GetAllGateSpec()
        {
            UseNoTracking();
            Select(x => new GetAllGateQueryResponse
            {
                GateId = x.Id,
                GateName = x.GateType == GateType.Entrance
                    ? $"{x.GateNumber} Entrance"
                    : $"{x.GateNumber} Exit",
                GateType = x.GateType
            });
        }
    }
}