namespace SoberNetwork.Core.DTOs.News;

/// <summary>Request to update an existing post. Null fields are left unchanged.</summary>
public record UpdatePostRequest(
    string? Subject,
    string? Body,
    string? ImageUrl,
    string? LinkUrl,
    string? LinkTitle
);
