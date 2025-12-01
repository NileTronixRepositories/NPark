using Microsoft.AspNetCore.SignalR;
using NPark.Application.Abstraction;
using NPark.Application.Shared.Dto;

namespace NPark.Api.RealTime
{
    /// <summary>
    /// SignalR-based implementation of IRealtimeNotifier.
    /// </summary>
    public sealed class RealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationsHub, IRealtimeNotificationClient> _hub;

        public RealtimeNotifier(
            IHubContext<NotificationsHub, IRealtimeNotificationClient> hub)
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        public Task NotifyTicketAddedAsync(TicketAddedNotification payload)
            => _hub.Clients.All.TicketAdded(payload);

        public Task NotifyTicketExitedAsync(TicketExitedNotification payload)
            => _hub.Clients.All.TicketExited(payload);
    }
}