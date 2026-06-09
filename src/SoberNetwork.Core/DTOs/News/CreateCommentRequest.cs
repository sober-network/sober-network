namespace SoberNetwork.Core.DTOs.News;

public record CreateCommentRequest(
    string Body,
    Guid? ParentCommentId = null,
    string? LinkUrl = null,
    string? LinkTitle = null
);
