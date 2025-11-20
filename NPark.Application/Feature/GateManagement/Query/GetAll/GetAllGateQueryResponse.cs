using NPark.Domain.Enums;

namespace NPark.Application.Feature.GateManagement.Query.GetAll
{
    public sealed record GetAllGateQueryResponse
    {
        public int GateName { get; init; }
        public GateType GateType { get; init; }
        public Guid GateId { get; init; }
    }
}
