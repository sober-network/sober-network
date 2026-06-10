using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoberNetwork.Core.DTOs.Notifications;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Infrastructure.Data;
using SoberNetwork.Infrastructure.Hubs;

namespace SoberNetwork.Infrastructure.Services;

public sealed class NotificationService(
    AppDbContext db,
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationService> logger) : INotificationService
{
    /// <inheritdoc/>
    public async Task CreateNotificationAsync(
        Guid recipientId,
        Guid triggerUserId,
        NotificationType type,
        Guid? postId = null,
        Guid? commentId = null,
        CancellationToken ct = default)
    {
        try
        {
            db.Notifications.Add(new Notification
            {
                RecipientId = recipientId,
                TriggerUserId = triggerUserId,
                Type = type,
                PostId = postId,
                CommentId = commentId,
            });
            await db.SaveChangesAsync(ct);

            var count = await GetUnreadCountAsync(recipientId, ct);
            await hubContext.Clients
                .User(recipientId.ToString())
                .SendAsync("notificationCount", count, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create notification type={Type} for recipient={RecipientId}", type, recipientId);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var notifications = await db.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new
            {
                n.Id,
                n.Type,
                n.PostId,
                PostSubject = n.Post != null ? n.Post.Subject : null,
                n.CommentId,
                n.TriggerUserId,
                TriggerUserDisplayName = n.TriggerUser.DisplayName,
                n.IsRead,
                n.CreatedAt,
            })
            .ToListAsync(ct);

        return notifications.Select(n => new NotificationResponse(
            n.Id,
            n.Type.ToString(),
            n.PostId,
            n.PostSubject,
            n.CommentId,
            n.TriggerUserId,
            n.TriggerUserDisplayName,
            n.IsRead,
            n.CreatedAt));
    }

    /// <inheritdoc/>
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => await db.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead, ct);

    /// <inheritdoc/>
    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        await db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}

