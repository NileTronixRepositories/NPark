using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public sealed record SelectGateCommand : ICommand<UserTokenDto>
    {
        public int GateNumber { get; init; }
        public GateType GateType { get; init; }
    }
}