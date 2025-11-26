using BuildingBlock.Application.Abstraction;
using NPark.Application.Feature.TicketsManagement.Command.CalculateSalary;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalaryForSubscriber
{
    public sealed record CalculateSalaryForSubscriberCommand : ICommand<CalculateSalaryCommandResponse>
    {
        public string CardNumber { get; init; } = string.Empty;
    }
}