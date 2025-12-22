using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.AccountManagement.Query.GetAll;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.AccountSpecification
{
    internal sealed class GetAllAccountSpec : Specification<Account, GetAllAccountQueryResponse>
    {
        public GetAllAccountSpec()
        {
            Include(x => x.Sites);
            Select(x => new GetAllAccountQueryResponse
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Sites = x.Sites.Select(x => new GetSiteDto { Id = x.Id, NameAr = x.NameAr, NameEn = x.NameEn }).ToList()
            });
            UseNoTracking();
        }
    }
}