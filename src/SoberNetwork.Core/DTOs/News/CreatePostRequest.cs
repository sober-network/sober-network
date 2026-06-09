namespace SoberNetwork.Core.DTOs.News;

/// <summary>Request to create a news/announcement post in a group.</summary>
public record CreatePostRequest(
    string GroupSlug,
    string Subject,
    string Body,
    string? ImageUrl = null,
    string? LinkUrl = null,
    string? LinkTitle = null
);
