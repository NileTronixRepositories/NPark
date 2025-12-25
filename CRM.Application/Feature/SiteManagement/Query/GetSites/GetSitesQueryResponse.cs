namespace CRM.Application.Feature.SiteManagement.Query.GetSites
{
    public sealed record GetSitesQueryResponse
    {
        public string NameEn { get; init; } = string.Empty;
        public Guid Id { get; init; }
    }
}