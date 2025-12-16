using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetAll
{
    public sealed record GetAllArchiveRequestsQueryResponse
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;

        public ArchiveAction ActionTaken { get; set; }
        public ArchiveType ActionType { get; set; }
        public string? Note { get; set; } = string.Empty;
        public string? RejectReason { get; set; } = string.Empty;
    }
}