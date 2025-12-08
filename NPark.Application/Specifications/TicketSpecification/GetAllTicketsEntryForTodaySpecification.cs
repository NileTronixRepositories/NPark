using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetEntryTicketForAdmin;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public sealed class GetAllTicketsEntryForTodaySpecification : Specification<Ticket, GetEntryTicketForAdminQueryResponse>
    {
        public GetAllTicketsEntryForTodaySpecification()
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);
            UseNoTracking();
            Select(x => new GetEntryTicketForAdminQueryResponse
            {
                isCollectedByCashier = x.IsCollected,
                Price = x.Price,
                TicketNumber = x.UniqueCode,
                StartDate = x.CreatedOnUtc,
                TicketInfo = x.VehicleNumber
            });
        }
    }
}