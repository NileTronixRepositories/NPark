namespace CRM.Application.Feature.AccountManagement.Query.GetAll
{
    public sealed record GetAllAccountQueryResponse
    {
        public Guid Id { get; init; }
        public List<GetSiteDto> Sites { get; init; } = new();
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
    }

    public sealed record GetSiteDto
    {
        public Guid Id { get; init; }
        public string NameEn { get; init; } = string.Empty;
        public string? NameAr { get; init; }
    }
}