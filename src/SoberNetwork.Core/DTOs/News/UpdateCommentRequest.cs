namespace SoberNetwork.Core.DTOs.News;

public record UpdateCommentRequest(
    string Body,
    string? LinkUrl = null,
    string? LinkTitle = null
);
