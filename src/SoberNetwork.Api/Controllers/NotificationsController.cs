using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.Commands.Notifications;
using SoberNetwork.Core.Queries.Notifications;

namespace SoberNetwork.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns recent notifications for the authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken = default)
    {
        var notifications = await mediator.Send(new GetNotificationsQuery(UserId), cancellationToken);
        return Ok(notifications);
    }

    /// <summary>Returns the unread notification count for the authenticated user.</summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var count = await mediator.Send(new GetUnreadCountQuery(UserId), cancellationToken);
        return Ok(new { count });
    }

    /// <summary>Marks all notifications as read for the authenticated user.</summary>
    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken = default)
    {
        await mediator.Send(new MarkNotificationsReadCommand(UserId), cancellationToken);
        return NoContent();
    }
}
