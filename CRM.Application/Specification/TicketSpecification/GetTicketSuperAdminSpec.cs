using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.TicketManagement.Query.GetAllSuperAdmin;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.TicketSpecification
{
    internal sealed class GetTicketSuperAdminSpec : Specification<Ticket, GetAllSuperAdminQueryResponse>
    {
        public GetTicketSuperAdminSpec(GetAllSuperAdminQuery req)
        {
            if (req.SearchText != null)
            {
                AddCriteria(x => x.Subject.Contains(req.SearchText) || x.Description.Contains(req.SearchText)
               || x.Email.Contains(req.SearchText) || x.PhoneNumber.Contains(req.SearchText));
            }
            if (req.Severity != null)
                AddCriteria(x => x.Severity == req.Severity);
            if (req.Status != null)
                AddCriteria(x => x.Status == req.Status);

            Select(x => new GetAllSuperAdminQueryResponse
            {
                Id = x.Id,
                Subject = x.Subject,
                Description = x.Description,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                CreatedAt = x.CreatedOnUtc,
                Severity = x.Severity,
                Status = x.Status,
                SiteNameEn = x.Site.NameEn,
                SiteNameAr = x.Site.NameAr,
                AccountNameEn = x.Site.Account.NameEn,
                AccountNameAr = x.Site.Account.NameAr,
                IsInSupport = (DateTime.Now < x.Site.SiteProducts.Where(u => u.SiteId == u.SiteId
                && u.ProductId == x.ProductId).First().SupportEndDate) ? true : false,
                IsInProducts = (x.Site.SiteProducts.Any(u => u.SiteId == u.SiteId && u.ProductId == x.ProductId))
            });
            ApplyPaging(req.PageNumber, req.PageSize);
            UseNoTracking();
        }
    }
}