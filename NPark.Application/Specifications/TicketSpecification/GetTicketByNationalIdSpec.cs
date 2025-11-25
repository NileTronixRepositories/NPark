using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public class GetTicketByNationalIdSpec : Specification<Ticket>
    {
        public GetTicketByNationalIdSpec(string nationalId)
        {
            AddCriteria(t => t.IsSubscriber && t.SubscriberNationalId == nationalId);
            AddOrderByDescending(x => x.CreatedOnUtc);
        }
    }
}