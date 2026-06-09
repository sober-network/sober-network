namespace SoberNetwork.Core.DTOs.News;

/// <summary>A comment on a news post, optionally nested under a parent comment (one level of threading).</summary>
public record CommentResponse(
    Guid Id,
    Guid PostId,
    Guid? ParentCommentId,
    Guid AuthorId,
    string AuthorDisplayName,
    string Body,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
