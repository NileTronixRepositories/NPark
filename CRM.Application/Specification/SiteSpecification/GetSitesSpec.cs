using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.SiteManagement.Query.GetSites;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.SiteSpecification
{
    internal sealed class GetSitesSpec : Specification<Site, GetSitesQueryResponse>
    {
        public GetSitesSpec(Guid id)
        {
            AddCriteria(x => x.AccountId == id);
            Select(x => new GetSitesQueryResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
            });
            UseNoTracking();
        }
    }
}