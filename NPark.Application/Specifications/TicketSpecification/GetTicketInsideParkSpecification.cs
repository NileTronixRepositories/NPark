using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketInsidePark;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public sealed class GetTicketInsideParkSpecification : Specification<Ticket, GetTicketInsideParkQueryResponse>
    {
        public GetTicketInsideParkSpecification()
        {
            AddCriteria(t => t.CreatedOnUtc >= DateTime.Now.Date && t.EndDate == null);
            Select(x => new GetTicketInsideParkQueryResponse
            {
                isCollectedByCashier = x.IsCashierCollected,
                Price = x.Price,
                StartDate = x.CreatedOnUtc,
                TicketNumber = x.UniqueCode,
                TicketInfo = x.VehicleNumber,
                Id = x.Id
            }
            );
        }
    }
}