using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.UserManagement.Command.Add
{
    public sealed class AddUserCommand : ICommand
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string NationalId { get; init; } = string.Empty;
        public Guid RoleId { get; init; }
    }
}