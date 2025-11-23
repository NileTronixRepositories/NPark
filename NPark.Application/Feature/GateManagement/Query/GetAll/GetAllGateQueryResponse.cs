using NPark.Domain.Enums;

namespace NPark.Application.Feature.GateManagement.Query.GetAll
{
    public sealed record GetAllGateQueryResponse
    {
        public string GateName { get; init; } = string.Empty;
        public GateType GateType { get; init; }
        public Guid GateId { get; init; }
    }
}