using BuildingBlock.Domain.Specification;
using CRM.Application.Shared.Dto;
using CRM.Domain.Entities;
using CRM.Domain.FileName;

namespace CRM.Application.Specification.SiteSpecification
{
    internal sealed class GetSitecByAccountIdWithAllInfo : Specification<Site, SharedSiteDto>
    {
        public GetSitecByAccountIdWithAllInfo(Guid? accountId, int pageNumber, int pageSize)
        {
            if (accountId.HasValue)
                AddCriteria(x => x.AccountId == accountId);

            Select(x => new SharedSiteDto
            {
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Id = x.Id,
                Products = x.SiteProducts.Select(p => new SharedProductDto
                {
                    Id = p.ProductId,
                    DescriptionEn = p.Product.DescriptionEn,
                    DescriptionAr = p.Product.DescriptionAr,
                    NameEn = p.Product.NameEn,
                    NameAr = p.Product.NameAr,
                    ProductImage = $"Media/{FileNames.Product}/{p.Product.ImagePath}"
                }).ToList(),
                Tickets = x.Tickets.Select(t => new SharedTicketDto
                {
                    Id = t.Id,
                    Description = t.Description,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    Severity = t.Severity,
                    Status = t.Status,
                    Subject = t.Subject,
                    Attachments = t.Attachments.Select(a => new SharedTicketAttachmentDto
                    {
                        Id = a.Id,
                        Url = $"Media/{FileNames.TicketAttachment}/{a.FilePath}"
                    }).ToList()
                }).ToList()
            });
            ApplyPaging(pageNumber, pageSize);

            UseNoTracking();
        }
    }
}