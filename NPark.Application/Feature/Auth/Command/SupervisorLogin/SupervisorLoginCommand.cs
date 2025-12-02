using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.Auth.Command.SupervisorLogin
{
    public sealed record SupervisorLoginCommand : ICommand<UserTokenDto>
    {
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public int GateNumber { get; init; }
        public GateType GateType { get; init; }
    }
}