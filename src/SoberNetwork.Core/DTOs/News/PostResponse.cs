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
    string? ImageUrl,
    string? LinkUrl,
    string? LinkTitle,
    bool IsApproved,
    /// <summary>True when the post is pending admin review (shown to admins only).</summary>
    bool NeedsApproval,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CommentCount
);
