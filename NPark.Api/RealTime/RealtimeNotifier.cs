using Microsoft.AspNetCore.SignalR;
using NPark.Application.Abstraction;

namespace NPark.Api.RealTime
{
    /// <summary>
    /// SignalR-based implementation of IRealtimeNotifier.
    /// </summary>
    public sealed class RealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public RealtimeNotifier(IHubContext<NotificationsHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public Task PublishAsync(string channel, object payload, CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.All.SendAsync(channel, payload, cancellationToken);
        }
    }
}