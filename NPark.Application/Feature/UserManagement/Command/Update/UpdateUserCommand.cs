using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.UserManagement.Command.Update
{
    public sealed record UpdateUserCommand : ICommand
    {
        public string Email { get; init; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public Guid Id { get; init; }
    }
}