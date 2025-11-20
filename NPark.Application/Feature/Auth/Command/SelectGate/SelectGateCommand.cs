using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public sealed record SelectGateCommand : ICommand<UserTokenDto>
    {
        public int GateNumber { get; init; }
    }
}