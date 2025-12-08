using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Query.GetExitTicketForAdmin
{
    public sealed class GetExitTicketForAdminQueryHandler : IQueryHandler<GetExitTicketForAdminQuery, IReadOnlyList<GetExitTicketForAdminQueryResponse>>
    {
        private readonly IGenericRepository<Ticket> _repo;

        public GetExitTicketForAdminQueryHandler(IGenericRepository<Ticket> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<Result<IReadOnlyList<GetExitTicketForAdminQueryResponse>>> Handle(GetExitTicketForAdminQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetAllTicketsExitForTodaySpecification();
            var respone = await _repo.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetExitTicketForAdminQueryResponse>>.Ok(respone);
        }
    }
}