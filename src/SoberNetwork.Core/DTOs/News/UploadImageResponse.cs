namespace SoberNetwork.Core.DTOs.News;

/// <summary>Response after successfully uploading media (image or video).</summary>
public record UploadImageResponse(
    Guid MediaId,
    string MediaUrl,
    /// <summary>Thumbnail URL (for videos only).</summary>
    string? ThumbnailUrl,
    long FileSizeBytes,
    string MediaType,  // "image" or "video"
    int? ImageWidth,
    int? ImageHeight,
    /// <summary>Video duration in seconds (videos only).</summary>
    int? VideoDurationSeconds
);
