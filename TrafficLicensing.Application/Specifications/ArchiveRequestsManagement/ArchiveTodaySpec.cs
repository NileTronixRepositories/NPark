using BuildingBlock.Domain.Specification;
using TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetToday;
using TrafficLicensing.Domain.Entities;

namespace TrafficLicensing.Application.Specifications.ArchiveRequestsManagement
{
    internal sealed class ArchiveTodaySpec : Specification<ArchiveRequests, GetArchiveTodayQueryResponse>
    {
        public ArchiveTodaySpec(GetArchiveTodayQuery request)
        {
            var todayStart = DateTime.Now.Date;
            var tomorrowStart = todayStart.AddDays(1);
            AddCriteria(x => x.CreatedOnUtc >= todayStart && x.CreatedOnUtc < tomorrowStart);
            if (request.SearchText != null)
            {
                AddCriteria(x => x.PlateNumber.Trim().Contains(request.SearchText.Trim()));
            }

            AddOrderByDescending(x => x.CreatedOnUtc);
            ApplyPaging(request.PageNumber, request.PageSize);

            Select(x => new GetArchiveTodayQueryResponse
            {
                Id = x.Id,
                PlateNumber = x.PlateNumber,
                ActionType = x.ActionType,
                ActionTaken = x.ActionTaken,
                Note = x.Note,
                RejectReason = x.RejectReason
            });
            UseNoTracking();
        }
    }
}