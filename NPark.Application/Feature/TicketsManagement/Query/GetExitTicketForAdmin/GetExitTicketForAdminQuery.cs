using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Query.GetExitTicketForAdmin
{
    public sealed record GetExitTicketForAdminQuery : IQuery<IReadOnlyList<GetExitTicketForAdminQueryResponse>>
    {
    }
}