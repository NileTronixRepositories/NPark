using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using TrafficLicensing.Application.Specifications.ArchiveRequestsManagement;
using TrafficLicensing.Domain.Entities;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetToday
{
    internal sealed class GetArchiveTodayQueryHandler : IQueryHandler<GetArchiveTodayQuery, Pagination<GetArchiveTodayQueryResponse>>
    {
        private readonly IGenericRepository<ArchiveRequests> _archiveRequestsRepository;

        public GetArchiveTodayQueryHandler(IGenericRepository<ArchiveRequests> archiveRequestsRepository)
        {
            _archiveRequestsRepository = archiveRequestsRepository ?? throw new ArgumentNullException(nameof(archiveRequestsRepository));
        }

        public async Task<Result<Pagination<GetArchiveTodayQueryResponse>>> Handle(GetArchiveTodayQuery request, CancellationToken cancellationToken)
        {
            var spec = new ArchiveTodaySpec(request);
            (var data, var count) = _archiveRequestsRepository.GetWithSpec(spec);
            var result = new Pagination<GetArchiveTodayQueryResponse>
                (request.PageNumber, request.PageSize, count, await data.ToListAsync(cancellationToken));
            return Result<Pagination<GetArchiveTodayQueryResponse>>.Ok(result);
        }
    }
}