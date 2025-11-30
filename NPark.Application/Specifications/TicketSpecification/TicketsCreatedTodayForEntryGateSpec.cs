using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public class TicketsCreatedTodayForEntryGateSpec : Specification<Ticket, GetTicketInfo>
    {
        public TicketsCreatedTodayForEntryGateSpec(Guid gateId)
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end && t.GateId == gateId);
            AddCriteria(x => !x.IsCollected);
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