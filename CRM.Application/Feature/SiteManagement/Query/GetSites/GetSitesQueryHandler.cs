using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Application.Abstraction.Security;
using CRM.Application.Specification.SiteSpecification;
using CRM.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace CRM.Application.Feature.SiteManagement.Query.GetSites
{
    internal sealed class GetSitesQueryHandler : IQueryHandler<GetSitesQuery, IReadOnlyList<GetSitesQueryResponse>>
    {
        private readonly IGenericRepository<Site> _siteRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;

        public GetSitesQueryHandler(IGenericRepository<Site> siteRepository, IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader)
        {
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
        }

        public async Task<Result<IReadOnlyList<GetSitesQueryResponse>>> Handle(GetSitesQuery request, CancellationToken cancellationToken)
        {
            var tokenInfo = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.UserId.HasValue)
            {
                return Result<IReadOnlyList<GetSitesQueryResponse>>.Fail(
                    new Error(
                        Code: "Token.GateOrUser.NotFound",
                        Message: "Token Missing Or User Not Found",
                        Type: ErrorType.NotFound));
            }
            var spec = new GetSitesSpec(tokenInfo.UserId!.Value);
            var response = await _siteRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetSitesQueryResponse>>.Ok(response);
        }
    }
}