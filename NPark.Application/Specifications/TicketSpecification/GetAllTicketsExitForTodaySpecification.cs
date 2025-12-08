using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetExitTicketForAdmin;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public sealed class GetAllTicketsExitForTodaySpecification : Specification<Ticket, GetExitTicketForAdminQueryResponse>
    {
        public GetAllTicketsExitForTodaySpecification()
        {
            var date = DateTime.Now.Date;
            var start = date;
            var end = date.AddDays(1);
            AddCriteria(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);
            AddCriteria(x => x.EndDate != null && x.EndDate.HasValue);
            UseNoTracking();
            Select(x => new GetExitTicketForAdminQueryResponse
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