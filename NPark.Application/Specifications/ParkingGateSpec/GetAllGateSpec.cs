using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.GateManagement.Query.GetAll;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Specifications.ParkingGateSpec
{
    public class GetAllGateSpec : Specification<ParkingGate, GetAllGateQueryResponse>
    {
        public GetAllGateSpec(string roleName)
        {
            UseNoTracking();
            if (roleName == "EntranceCashier")
            {
                AddCriteria(x => x.GateType == GateType.Entrance);
            }
            else if (roleName == "ExitCashier")
            {
                AddCriteria(x => x.GateType == GateType.Exit);
            }
            Select(x => new GetAllGateQueryResponse
            {
                GateId = x.Id,
                GateName = x.GateType == GateType.Entrance
                    ? $"{ErrorMessage.EntranceGate} {x.GateNumber}"
                    : $"{ErrorMessage.ExitGate} {x.GateNumber} ",
                GateType = x.GateType
            });
        }
    }
}