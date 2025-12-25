using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Specification.TicketSpecification;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Feature.TicketManagement.Query.GetAllSuperAdmin
{
    internal sealed class GetAllSuperAdminQueryHandler : IQueryHandler<GetAllSuperAdminQuery, Pagination<GetAllSuperAdminQueryResponse>>
    {
        private readonly IGenericRepository<Ticket> _genericRepository;

        public GetAllSuperAdminQueryHandler(IGenericRepository<Ticket> genericRepository)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
        }

        public async Task<Result<Pagination<GetAllSuperAdminQueryResponse>>> Handle(GetAllSuperAdminQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetTicketSuperAdminSpec(request);

            var tickets = _genericRepository.GetWithSpec(spec);
            var response = new Pagination<GetAllSuperAdminQueryResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                data: await tickets.data.ToListAsync(cancellationToken),
                totalItems: tickets.count);
            return Result<Pagination<GetAllSuperAdminQueryResponse>>.Ok(response);
        }
    }
}