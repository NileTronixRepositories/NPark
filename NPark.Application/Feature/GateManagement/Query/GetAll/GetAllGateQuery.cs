using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.GateManagement.Query.GetAll
{
    public sealed record GetAllGateQuery : IQuery<IReadOnlyList<GetAllGateQueryResponse>>
    {
    }
}
