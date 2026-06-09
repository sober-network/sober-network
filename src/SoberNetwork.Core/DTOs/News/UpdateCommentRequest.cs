namespace SoberNetwork.Core.DTOs.News;

public record UpdateCommentRequest(
    string Body,
    string? ImageUrl = null,
    string? LinkUrl = null,
    string? LinkTitle = null
);
