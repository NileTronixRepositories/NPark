using BuildingBlock.Domain.EntitiesHelper;
using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Domain.Entities
{
    public sealed class ArchiveRequests : Entity<Guid>
    {
        public string PlateNumber { get; private set; } = string.Empty;

        public ArchiveAction ActionTaken { get; private set; }
        public ArchiveType ActionType { get; private set; }
        public string? Note { get; private set; } = string.Empty;
        public string? RejectReason { get; private set; } = string.Empty;

        private ArchiveRequests()
        { }

        public static ArchiveRequests Create(string plateNumber, ArchiveType archiveType, string? note = null, string? rejectReason = null)
        {
            return new ArchiveRequests
            {
                Id = Guid.NewGuid(),
                ActionType = archiveType,
                PlateNumber = plateNumber,
                ActionTaken = ArchiveAction.Pending,
                Note = note,
                RejectReason = rejectReason
            };
        }

        public void SetAction(ArchiveAction action, string? rejectReason = null)
        {
            ActionTaken = action;
            RejectReason = rejectReason;
        }

        public void SetReason(string reason)
        {
            RejectReason = reason;
        }
    }
}