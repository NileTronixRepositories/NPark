using BuildingBlock.Application.Abstraction;
using CRM.Application.Shared.Dto;

namespace CRM.Application.Feature.AuthManagement.Command.Login
{
    public sealed record LoginCommand : ICommand<UserTokenDto>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}