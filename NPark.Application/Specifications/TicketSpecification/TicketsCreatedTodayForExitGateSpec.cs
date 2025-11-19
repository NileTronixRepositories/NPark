using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    internal class TicketsCreatedTodayForExitGateSpec : Specification<Ticket>
    {
        public TicketsCreatedTodayForExitGateSpec()
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);
            AddCriteria(x => x.IsCollected);
            UseNoTracking();
        }
    }
}