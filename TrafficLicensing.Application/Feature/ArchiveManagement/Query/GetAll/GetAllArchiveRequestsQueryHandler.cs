using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using TrafficLicensing.Application.Specifications.ArchiveRequestsManagement;
using TrafficLicensing.Domain.Entities;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetAll
{
    internal sealed class GetAllArchiveRequestsQueryHandler : IQueryHandler<GetAllArchiveRequestsQuery, Pagination<GetAllArchiveRequestsQueryResponse>>
    {
        private readonly IGenericRepository<ArchiveRequests> _archiveRequestsRepository;

        public GetAllArchiveRequestsQueryHandler(IGenericRepository<ArchiveRequests> archiveRequestsRepository)
        {
            _archiveRequestsRepository = archiveRequestsRepository ?? throw new ArgumentNullException(nameof(archiveRequestsRepository));
        }

        public async Task<Result<Pagination<GetAllArchiveRequestsQueryResponse>>> Handle(GetAllArchiveRequestsQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetAllArchiveManagementSpec(request);
            (var data, var count) = _archiveRequestsRepository.GetWithSpec(spec);
            var result = new Pagination<GetAllArchiveRequestsQueryResponse>
                (request.PageNumber, request.PageSize, count, await data.ToListAsync(cancellationToken));
            return Result<Pagination<GetAllArchiveRequestsQueryResponse>>.Ok(result);
        }
    }
}