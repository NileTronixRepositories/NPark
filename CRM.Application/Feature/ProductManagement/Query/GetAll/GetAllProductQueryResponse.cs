namespace CRM.Application.Feature.ProductManagement.Query.GetAll
{
    public sealed record GetAllProductQueryResponse
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
        public string? DescriptionEn { get; init; }
        public string? DescriptionAr { get; init; }
        public string? ImagePath { get; init; }
    }
}