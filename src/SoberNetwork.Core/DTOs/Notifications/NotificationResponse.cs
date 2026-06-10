namespace SoberNetwork.Core.DTOs.Notifications;

public record NotificationResponse(
    Guid Id,
    string Type,
    Guid? PostId,
    string? PostSubject,
    Guid? CommentId,
    Guid TriggerUserId,
    string TriggerUserDisplayName,
    bool IsRead,
    DateTime CreatedAt
);
