namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public sealed record CalculateSalaryCommandResponse
    {
        public decimal TotalSalary { get; init; }
        public DateTime EnterDate { get; init; }
        public bool IsExitValid { get; init; }
        public bool IsCollectByCashier { get; init; }
    }
}