namespace SoberNetwork.Core.DTOs.News;

/// <summary>A news/announcement post visible to group members.</summary>
public record PostResponse(
    Guid Id,
    Guid GroupId,
    string GroupName,
    string GroupSlug,
    Guid AuthorId,
    string AuthorDisplayName,
    string Subject,
    string Body,
    /// <summary>URL to the uploaded media (image or video).</summary>
    string? MediaUrl,
    /// <summary>Thumbnail URL (videos only).</summary>
    string? ThumbnailUrl,
    /// <summary>Media type: "image" or "video".</summary>
    string? MediaType,
    /// <summary>Dimensions of the uploaded media.</summary>
    int? ImageWidth,
    int? ImageHeight,
    /// <summary>Video duration in seconds (videos only).</summary>
    int? VideoDurationSeconds,
    string? LinkUrl,
    string? LinkTitle,
    bool IsApproved,
    /// <summary>True when the post is pending admin review (shown to admins only).</summary>
    bool NeedsApproval,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CommentCount,
    int LikeCount,
    bool IsLikedByCurrentUser
);
