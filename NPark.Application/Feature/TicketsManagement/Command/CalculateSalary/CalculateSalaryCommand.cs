using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public sealed record CalculateSalaryCommand : ICommand<CalculateSalaryCommandResponse>
    {
        public string QrCode { get; init; } = string.Empty;
    }
}