using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.RoleManagement.Query.GetRoles
{
    public sealed record GetRolesQuery : IQuery<IReadOnlyList<GetRolesQueryResponse>>
    {
    }
}