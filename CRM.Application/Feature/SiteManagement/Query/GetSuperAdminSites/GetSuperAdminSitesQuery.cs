using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Shared.Dto;

namespace CRM.Application.Feature.SiteManagement.Query.GetSuperAdminSites
{
    public sealed class GetSuperAdminSitesQuery : SearchParameters, IQuery<Pagination<SharedSiteDto>>
    {
    }
}