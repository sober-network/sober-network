using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SoberNetwork.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery.
/// Clients connect authenticated; the server pushes "notificationCount" updates
/// via IHubContext when new notifications are created.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    // No client-callable methods needed — push-only from server.
}
