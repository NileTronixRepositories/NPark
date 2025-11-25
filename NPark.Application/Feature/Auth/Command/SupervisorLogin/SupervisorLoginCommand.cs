using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.Auth.Command.SupervisorLogin
{
    public sealed record SupervisorLoginCommand : ICommand<UserTokenDto>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int GateNumber { get; set; }
        public GateType GateType { get; set; }
    }
}