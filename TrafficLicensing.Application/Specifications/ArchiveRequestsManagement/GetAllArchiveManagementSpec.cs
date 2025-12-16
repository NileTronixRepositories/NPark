using BuildingBlock.Domain.Specification;
using TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetAll;
using TrafficLicensing.Domain.Entities;
using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Application.Specifications.ArchiveRequestsManagement
{
    internal sealed class GetAllArchiveManagementSpec : Specification<ArchiveRequests, GetAllArchiveRequestsQueryResponse>
    {
        public GetAllArchiveManagementSpec(GetAllArchiveRequestsQuery request)
        {
            if (request.SearchText != null)
            {
                AddCriteria(x => x.PlateNumber.Trim().Contains(request.SearchText.Trim()));
            }
            AddCriteria(x => x.ActionType != ArchiveType.Other);

            AddOrderByDescending(x => x.CreatedOnUtc);
            ApplyPaging(request.PageNumber, request.PageSize);

            Select(x => new GetAllArchiveRequestsQueryResponse
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