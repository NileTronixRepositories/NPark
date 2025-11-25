using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public sealed class CalculateSalaryCommand : ICommand<CalculateSalaryCommandResponse>
    {
        public string QrCode { get; set; } = string.Empty;
    }
}