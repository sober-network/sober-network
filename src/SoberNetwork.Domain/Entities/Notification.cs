using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The user who receives this notification.</summary>
    public Guid RecipientId { get; set; }
    public ApplicationUser Recipient { get; set; } = null!;

    /// <summary>The user whose action triggered this notification.</summary>
    public Guid TriggerUserId { get; set; }
    public ApplicationUser TriggerUser { get; set; } = null!;

    public NotificationType Type { get; set; }

    /// <summary>The post this notification relates to (nullable for non-post notifications).</summary>
    public Guid? PostId { get; set; }
    public Post? Post { get; set; }

    /// <summary>The comment that triggered the notification (if applicable).</summary>
    public Guid? CommentId { get; set; }
    public PostComment? Comment { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
