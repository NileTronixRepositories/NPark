using BuildingBlock.Application.Abstraction;

namespace CRM.Application.Feature.AccountManagement.Command.Add
{
    public sealed record AddAccountCommand : ICommand
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public List<SiteDto> Sites { get; init; } = new();
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
    }
    public sealed record SiteDto
    {
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
        public List<AddProductDto>? Products { get; init; } = new(); // <ProductId, SupportEndDate>
    }
    public sealed record AddProductDto
    {
        public Guid ProductId { get; init; }
        public DateTime SupportEndDate { get; init; }
    }
}