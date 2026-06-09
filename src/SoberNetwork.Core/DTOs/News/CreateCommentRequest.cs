namespace SoberNetwork.Core.DTOs.News;

public record CreateCommentRequest(
    string Body,
    Guid? ParentCommentId = null,
    string? ImageUrl = null,
    string? LinkUrl = null,
    string? LinkTitle = null
);
