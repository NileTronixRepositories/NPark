using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    internal class TicketsCreatedTodayForExitGateSpec : Specification<Ticket, GetTicketInfo>
    {
        public TicketsCreatedTodayForExitGateSpec()
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);
            AddCriteria(x => x.EndDate != null && x.EndDate.HasValue && !x.IsCollected);
            UseNoTracking();
            Select(x => new GetTicketInfo
            {
                isCollectedByCashier = x.IsCollected,
                Price = x.Price,
                TicketNumber = BitConverter.ToString(x.UniqueGuidPart).Replace("-", ""),
                StartDate = x.CreatedOnUtc,
                TicketInfo = x.VehicleNumber
            });
        }
    }
}