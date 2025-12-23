using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.AccountManagement.Query.GetAll;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.AccountSpecification
{
    internal sealed class GetAllAccountSpec : Specification<Account, GetAllAccountQueryResponse>
    {
        public GetAllAccountSpec(GetAllAccountQuery request)
        {
            Include(x => x.Sites);
            Select(x => new GetAllAccountQueryResponse
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Sites = x.Sites.Select(x => new GetSiteDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Products = x.SiteProducts.Select(x => new GetProductsDto
                    {
                        Id = x.ProductId,
                        NameEn = x.Product.NameEn,
                        SupportDateEnd = x.SupportEndDate
                    }).ToList()
                }).ToList()
            });
            ApplyPaging(request.PageNumber, request.PageSize);
            UseNoTracking();
        }
    }
}