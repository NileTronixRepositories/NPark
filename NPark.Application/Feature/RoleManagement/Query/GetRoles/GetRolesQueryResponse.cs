namespace NPark.Application.Feature.RoleManagement.Query.GetRoles
{
    public sealed record GetRolesQueryResponse
    {
        public string RoleName { get; init; } = string.Empty;
        public Guid RoleId { get; init; }
    }
}