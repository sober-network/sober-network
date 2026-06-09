namespace SoberNetwork.Core.DTOs.News;

/// <summary>A comment on a news post, optionally nested under a parent comment (unlimited depth).</summary>
public record CommentResponse(
    Guid Id,
    Guid PostId,
    Guid? ParentCommentId,
    Guid AuthorId,
    string AuthorDisplayName,
    string Body,
    string? MediaUrl,
    string? MediaType,
    Guid? MediaId,
    string? LinkUrl,
    string? LinkTitle,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
