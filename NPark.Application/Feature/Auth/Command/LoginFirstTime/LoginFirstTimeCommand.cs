using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.Auth.Command.LoginFirstTime
{
    public sealed record LoginFirstTimeCommand : ICommand
    {
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
        public string ConfirmedPassword { get; init; } = string.Empty;
    }
}