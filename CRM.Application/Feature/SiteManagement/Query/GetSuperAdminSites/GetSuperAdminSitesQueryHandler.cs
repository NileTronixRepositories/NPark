using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Shared.Dto;
using CRM.Application.Specification.SiteSpecification;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Feature.SiteManagement.Query.GetSuperAdminSites
{
    internal class GetSuperAdminSitesQueryHandler : IQueryHandler<GetSuperAdminSitesQuery, Pagination<SharedSiteDto>>
    {
        private readonly IGenericRepository<Site> _repository;

        public GetSuperAdminSitesQueryHandler(IGenericRepository<Site> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result<Pagination<SharedSiteDto>>> Handle(GetSuperAdminSitesQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetSitecByAccountIdWithAllInfo(null, request.PageNumber, request.PageSize);
            var sites = _repository.GetWithSpec(spec);
            var response = new Pagination<SharedSiteDto>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                data: await sites.data.ToListAsync(cancellationToken),
                totalItems: sites.count);
            return Result<Pagination<SharedSiteDto>>.Ok(response);
        }
    }
}