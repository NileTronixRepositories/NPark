using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;

namespace NPark.Application.Feature.Auth.Command.Login
{
    public sealed record class LoginCommand : ICommand<UserTokenDto>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        //public GateType GateType { get; set; }
        //public int GateNumber { get; set; }
    }
}