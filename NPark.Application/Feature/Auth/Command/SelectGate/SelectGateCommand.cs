using BuildingBlock.Application.Abstraction;
using NPark.Application.Shared.Dto;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public sealed record SelectGateCommand : ICommand<UserTokenDto>
    {
        public Guid GateId { get; init; }
    }
}