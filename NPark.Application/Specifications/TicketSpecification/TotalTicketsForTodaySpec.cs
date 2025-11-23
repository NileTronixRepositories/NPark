using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public class TotalTicketsForTodaySpec : Specification<Ticket>
    {
        public TotalTicketsForTodaySpec(Guid gateId)
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end && t.GateId == gateId);
            EnableTotalCount();
        }
    }
}