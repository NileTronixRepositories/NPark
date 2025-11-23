using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.ColletByCachier
{
    public sealed record ColletByCachierCommand : ICommand
    {
        public Guid Id { get; init; }
    }
}