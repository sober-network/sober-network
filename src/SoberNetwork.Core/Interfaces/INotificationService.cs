using SoberNetwork.Core.DTOs.Notifications;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Creates and retrieves user notifications. Domain-agnostic delivery pipe — callers resolve recipients.</summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a notification and pushes the updated unread count to the recipient via SignalR.
    /// All context parameters are optional; callers supply only what's relevant to the notification type.
    /// </summary>
    Task CreateNotificationAsync(
        Guid recipientId,
        Guid triggerUserId,
        NotificationType type,
        Guid? postId = null,
        Guid? commentId = null,
        CancellationToken ct = default);

    /// <summary>Returns recent notifications for the given user, newest first.</summary>
    Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(
        Guid userId, CancellationToken ct = default);

    /// <summary>Returns the unread notification count for the given user.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Marks all notifications as read for the given user.</summary>
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}
