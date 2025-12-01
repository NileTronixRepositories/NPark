using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.GateManagement.Query.GetAll
{
    public sealed class GetAllGateQueryHandler : IQueryHandler<GetAllGateQuery, IReadOnlyList<GetAllGateQueryResponse>>
    {
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly ILogger<GetAllGateQueryHandler> _logger;

        public GetAllGateQueryHandler(IGenericRepository<ParkingGate> parkingGateRepository,
            IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader,
            ILogger<GetAllGateQueryHandler> logger)
        {
            _parkingGateRepository = parkingGateRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenReader = tokenReader;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<GetAllGateQueryResponse>>> Handle(GetAllGateQuery request, CancellationToken cancellationToken)
        {
            // ---------------------------
            // 2) Read token (GateId + UserId)
            // ---------------------------
            var tokenInfo = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.UserId.HasValue || string.IsNullOrEmpty(tokenInfo.Role))
            {
                _logger.LogWarning("Token info missing UserId or Role ");
                return Result<IReadOnlyList<GetAllGateQueryResponse>>.Fail(
                    new Error(
                        Code: "Token.GateOrUser.NotFound",
                        Message: ErrorMessage.TokenInfo_Missing,
                        Type: ErrorType.NotFound));
            }
            var spec = new GetAllGateSpec(tokenInfo.Role);
            var result = await _parkingGateRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetAllGateQueryResponse>>.Ok(result);
        }
    }
}