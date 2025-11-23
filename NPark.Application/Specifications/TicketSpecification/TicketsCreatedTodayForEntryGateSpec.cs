using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public class TicketsCreatedTodayForEntryGateSpec : Specification<Ticket, GetTicketForEntryGateQueryResponse>
    {
        public TicketsCreatedTodayForEntryGateSpec()
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);
            AddCriteria(x => !x.IsCollected);
            UseNoTracking();
            Select(x => new GetTicketForEntryGateQueryResponse
            {
                StartDate = x.CreatedOnUtc,
                TicketNumber = x.Id,
                Price = x.Price,
                isCollectedByCashier = x.IsCashierCollected
            });
        }
    }
}