namespace SoberNetwork.Core.DTOs.News;

public record UpdateCommentRequest(
    string Body,
    Guid? MediaId = null,
    string? LinkUrl = null,
    string? LinkTitle = null
);
