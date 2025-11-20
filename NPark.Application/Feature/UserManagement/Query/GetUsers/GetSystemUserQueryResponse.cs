namespace NPark.Application.Feature.UserManagement.Query.GetUsers
{
    public sealed record GetSystemUserQueryResponse
    {
        public string Username { get; init; } = string.Empty!;
        public string Email { get; init; } = string.Empty!;
        public string RoleName { get; init; } = string.Empty!;
    }
}