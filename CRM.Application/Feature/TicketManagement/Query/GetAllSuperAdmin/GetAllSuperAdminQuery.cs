using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CRM.Domain.Enums;

namespace CRM.Application.Feature.TicketManagement.Query.GetAllSuperAdmin
{
    public sealed class GetAllSuperAdminQuery : SearchParameters, IQuery<Pagination<GetAllSuperAdminQueryResponse>>
    {
        public TicketStatus? Status { get; init; }
        public TicketSeverity? Severity { get; init; }
    }
}