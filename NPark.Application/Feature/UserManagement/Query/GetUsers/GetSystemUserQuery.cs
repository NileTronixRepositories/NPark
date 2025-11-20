using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.UserManagement.Query.GetUsers
{
    public sealed record GetSystemUserQuery : IQuery<IReadOnlyList<GetSystemUserQueryResponse>>
    {
    }
}