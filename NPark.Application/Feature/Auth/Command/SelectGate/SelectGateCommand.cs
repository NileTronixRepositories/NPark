using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public sealed record SelectGateCommand : ICommand<UserTokenDto>
    {
        public string GateNumber { get; init; } = string.Empty;
        public GateType GateType { get; init; }
    }
}