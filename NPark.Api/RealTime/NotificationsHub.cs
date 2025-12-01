using Microsoft.AspNetCore.SignalR;
using NPark.Application.Abstraction;

namespace NPark.Api.RealTime
{
    public sealed class NotificationsHub : Hub<IRealtimeNotificationClient>
    {
    }
}