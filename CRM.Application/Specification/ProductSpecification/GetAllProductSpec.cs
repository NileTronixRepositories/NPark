using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.ProductManagement.Query.GetAll;
using CRM.Domain.Entities;
using CRM.Domain.FileName;

namespace CRM.Application.Specification.ProductSpecification
{
    public class GetAllProductSpec : Specification<Product, GetAllProductQueryResponse>
    {
        public GetAllProductSpec(GetAllProductQuery request)
        {
            Select(x => new GetAllProductQueryResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
                DescriptionEn = x.DescriptionEn,
                DescriptionAr = x.DescriptionAr,
                NameAr = x.NameAr,
                ImagePath = $"Media/{FileNames.Product}/{x.ImagePath}"
            });
            ApplyPaging(request.PageNumber, request.PageSize);
            UseNoTracking();
        }
    }
}