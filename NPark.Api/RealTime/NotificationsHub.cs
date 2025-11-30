using Microsoft.AspNetCore.SignalR;

namespace NPark.Api.RealTime
{
    /// <summary>
    /// Generic notifications hub. Projects can use channels like "tickets:added", "users:changed", etc.
    /// </summary>
    public sealed class NotificationsHub : Hub
    {
    }
}