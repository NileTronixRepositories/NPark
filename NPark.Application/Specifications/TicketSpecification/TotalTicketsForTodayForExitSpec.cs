using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public sealed class TotalTicketsForTodayForExitSpec : Specification<Ticket>
    {
        public TotalTicketsForTodayForExitSpec(Guid gateId)
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end && t.ExitGateId == gateId
            && !t.IsCollected);
            EnableTotalCount();
        }
    }
}