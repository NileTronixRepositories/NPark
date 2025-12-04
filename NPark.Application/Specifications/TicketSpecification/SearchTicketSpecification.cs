using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public sealed class SearchTicketSpecification : Specification<Ticket, GetTicketInfo>
    {
        public SearchTicketSpecification(SearchParameters searchParameters)
        {
            if (!string.IsNullOrEmpty(searchParameters.SearchText))
                AddCriteria(t => (t.VehicleNumber != null && t.VehicleNumber.Contains(searchParameters.SearchText)) ||
                t.UniqueCode.Contains(searchParameters.SearchText) ||
                (t.CardNumber != null && t.CardNumber.Contains(searchParameters.SearchText)));

            if (searchParameters.OrderSort == OrderSort.Newest)
            {
                AddOrderBy(x => x.CreatedOnUtc);
            }
            else
            {
                AddOrderByDescending(x => x.CreatedOnUtc);
            }

            Select(x => new GetTicketInfo
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