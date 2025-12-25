using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Shared.Dto;

namespace CRM.Application.Feature.SiteManagement.Query.GetAll
{
    public sealed class GetAllSitesQuery : SearchParameters, IQuery<Pagination<SharedSiteDto>>
    {
    }
}