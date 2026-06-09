namespace SoberNetwork.Core.Interfaces;

/// <summary>Handles image and video upload, validation, optimization, and storage.</summary>
public interface IMediaService
{
    /// <summary>
    /// Uploads and optimizes media (image or video).
    /// 
    /// Images (5 MB max, JPEG/PNG only):
    /// - Validate size and MIME type
    /// - Strip EXIF data (privacy — Tradition 12)
    /// - Resize/compress
    /// 
    /// Videos (50 MB max, MP4 only):
    /// - Validate size and MIME type
    /// - Transcode to H.264 MP4 (browser compatibility)
    /// - Generate thumbnail
    /// </summary>
    /// <param name="groupId">Tenant isolation — group that owns the media.</param>
    /// <param name="postId">Post this media is attached to.</param>
    /// <param name="fileStream">File content stream.</param>
    /// <param name="fileName">Original filename (will be sanitized).</param>
    /// <param name="contentType">MIME type (e.g., "image/jpeg", "video/mp4").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result with URL and metadata, or error.</returns>
    Task<Result<UploadMediaResult>> UploadMediaAsync(
        Guid groupId,
        Guid postId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an uploaded media file (soft delete).
    /// </summary>
    /// <param name="groupId">Verify group ownership.</param>
    /// <param name="mediaId">Media to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or error result.</returns>
    Task<Result<bool>> DeleteMediaAsync(
        Guid groupId,
        Guid mediaId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the public URL for an uploaded media file.
    /// </summary>
    /// <param name="groupId">Verify group access.</param>
    /// <param name="mediaId">Media to retrieve URL for.</param>
    /// <returns>Public URL or null if not found.</returns>
    Task<string?> GetMediaUrlAsync(Guid groupId, Guid mediaId);
}

/// <summary>Result of a successful media upload.</summary>
public record UploadMediaResult(
    Guid MediaId,
    string MediaUrl,
    /// <summary>Thumbnail URL (for videos only; null for images).</summary>
    string? ThumbnailUrl,
    long FileSizeBytes,
    string MediaType,  // "image" or "video"
    int? ImageWidth,    // Images and video thumbnails
    int? ImageHeight,
    int? VideoDurationSeconds);  // Videos only

/// <summary>Generic result wrapper for operations.</summary>
public record Result<T>(bool Success, T? Data, string? Error)
{
    public static Result<T> Ok(T data) => new(true, data, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
