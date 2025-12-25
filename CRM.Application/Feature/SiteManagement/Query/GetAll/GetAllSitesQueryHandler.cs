using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Abstraction.Security;
using CRM.Application.Shared.Dto;
using CRM.Application.Specification.SiteSpecification;
using CRM.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Feature.SiteManagement.Query.GetAll
{
    internal sealed class GetAllSitesQueryHandler : IQueryHandler<GetAllSitesQuery, Pagination<SharedSiteDto>>
    {
        private readonly IGenericRepository<Site> _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;

        public GetAllSitesQueryHandler(IGenericRepository<Site> repository, IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
        }

        public async Task<Result<Pagination<SharedSiteDto>>> Handle(GetAllSitesQuery request, CancellationToken cancellationToken)
        {
            var tokenInfo = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.UserId.HasValue)
            {
                return Result<Pagination<SharedSiteDto>>.Fail(
                    new Error(
                        Code: "Token.GateOrUser.NotFound",
                        Message: "Token Missing Or User Not Found",
                        Type: ErrorType.NotFound));
            }
            var spec = new GetSitecByAccountIdWithAllInfo(tokenInfo.UserId.Value, request.PageNumber, request.PageSize);
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