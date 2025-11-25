using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate
{
    public sealed class GetTicketForEntryGateQueryHandler : IQueryHandler<GetTicketForEntryGateQuery, GetTicketForEntryGateQueryResponse>
    {
        private readonly IGenericRepository<Ticket> _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;

        public GetTicketForEntryGateQueryHandler(IGenericRepository<Ticket> repository,
            IHttpContextAccessor httpContextAccessor, ITokenReader tokenReader)
        {
            _repo = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
        }

        public async Task<Result<GetTicketForEntryGateQueryResponse>> Handle(GetTicketForEntryGateQuery request, CancellationToken cancellationToken)
        {
            var tokenInfo = _httpContextAccessor!.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
            {
                return Result<GetTicketForEntryGateQueryResponse>.Fail(new Error("GateId not found", "GateId not found", ErrorType.NotFound));
            }
            var spec = new TicketsCreatedTodayForEntryGateSpec(tokenInfo.GateId.Value);
            var ticketsInfo = await _repo.ListWithSpecAsync(spec);
            var result = new GetTicketForEntryGateQueryResponse
            {
                TicketInfo = ticketsInfo
            };
            result.TotalPrice = result.TicketInfo.Sum(x => x.Price);
            return Result<GetTicketForEntryGateQueryResponse>
                .Ok(result);
        }
    }
}