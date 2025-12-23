namespace CRM.Application.Feature.ProductManagement.Query.GetProducts
{
    public sealed record GetProductsResponse
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = string.Empty;
    }
}