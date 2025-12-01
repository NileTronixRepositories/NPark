using NPark.Application.Shared.Dto;

namespace NPark.Application.Abstraction
{
    public interface IRealtimeNotificationClient
    {
        Task TicketAdded(TicketAddedNotification payload);

        Task TicketExited(TicketExitedNotification payload);
    }
}