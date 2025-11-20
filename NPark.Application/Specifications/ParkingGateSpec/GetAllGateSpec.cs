using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.GateManagement.Query.GetAll;
using NPark.Domain.Entities;

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
                GateName = x.GateNumber,
                GateType = x.GateType
            });
        }
    }
}
