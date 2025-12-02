using BuildingBlock.Application.Abstraction;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.PricingSchemaManagement.Command.Add
{
    public sealed record AddPricingSchemaCommand : ICommand
    {
        public string Name { get; init; } = string.Empty;
        public DurationType DurationType { get; init; }
        public TimeSpan? StartTime { get; init; }
        public TimeSpan? EndTime { get; init; }
        public decimal Price { get; init; }
        public bool IsRepeated { get; init; }
        public int? TotalDays { get; init; }
        public int? TotalHours { get; init; }
    }
}