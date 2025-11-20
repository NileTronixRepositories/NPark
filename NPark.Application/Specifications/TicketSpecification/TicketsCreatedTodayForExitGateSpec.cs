using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    internal class TicketsCreatedTodayForExitGateSpec : Specification<Ticket, GetTicketForExitGateQueryResponse>
    {
        public TicketsCreatedTodayForExitGateSpec()
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);
            AddCriteria(x => x.IsCollected);
            UseNoTracking();
            Select(x => new GetTicketForExitGateQueryResponse
            {
                StartDate = x.CreatedOnUtc,
                TicketNumber = x.Id
            });

        }
    }
}