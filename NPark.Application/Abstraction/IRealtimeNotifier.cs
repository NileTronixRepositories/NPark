using NPark.Application.Shared.Dto;

namespace NPark.Application.Abstraction
{
    public interface IRealtimeNotifier
    {
        Task NotifyTicketAddedAsync(TicketAddedNotification payload);

        Task NotifyTicketExitedAsync(TicketExitedNotification payload);
    }
}