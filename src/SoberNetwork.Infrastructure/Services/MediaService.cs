using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

/// <summary>
/// Media upload/storage service. Handles image (JPEG/PNG) and video (MP4) uploads with processing.
/// Images: EXIF stripped, resized to max 1920px width, quality compressed.
/// Videos: Requires external FFmpeg transcoding (stubbed for now).
/// Storage: Local files initially; extend with Supabase Storage backend.
/// </summary>
public sealed class MediaService(AppDbContext db, ILogger<MediaService> logger) : IMediaService
{
    private readonly string _uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "media");

    /// <inheritdoc/>
    public async Task<Result<UploadMediaResult>> UploadMediaAsync(
        Guid groupId, Guid postId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            // Ensure upload directory exists
            Directory.CreateDirectory(_uploadDir);

            // Determine media type and validate
            var mediaType = contentType.StartsWith("image/") ? "image" : "video";
            var (isValid, error) = ValidateFile(fileStream, contentType, mediaType);
            if (!isValid)
                return Result<UploadMediaResult>.Fail(error ?? "Invalid file.");

            // Reset stream position before processing
            fileStream.Position = 0;

            // Process and save file
            var (storagePath, thumbnailPath, metadata) = await ProcessAndSaveMediaAsync(
                fileStream, fileName, contentType, mediaType, cancellationToken);

            // Create database entity
            var media = new PostMedia
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                PostId = null,  // Will be linked when post is created
                MediaType = mediaType,
                FileName = fileName,
                StoragePath = storagePath,
                ThumbnailPath = thumbnailPath,
                ContentType = contentType,
                FileSizeBytes = fileStream.Length,
                ImageWidth = metadata.Width,
                ImageHeight = metadata.Height,
                VideoDurationSeconds = metadata.DurationSeconds,
                CreatedAt = DateTime.UtcNow,
            };

            db.PostMedia.Add(media);
            await db.SaveChangesAsync(cancellationToken);

            // Return public URLs (these would be Supabase signed URLs in production)
            var mediaUrl = $"/uploads/media/{media.Id}/{fileName}";
            var thumbUrl = thumbnailPath != null ? $"/uploads/media/{media.Id}/thumb-{fileName}" : null;

            var result = new UploadMediaResult(
                MediaId: media.Id,
                MediaUrl: mediaUrl,
                ThumbnailUrl: thumbUrl,
                FileSizeBytes: fileStream.Length,
                MediaType: mediaType,
                ImageWidth: metadata.Width,
                ImageHeight: metadata.Height,
                VideoDurationSeconds: metadata.DurationSeconds);

            return Result<UploadMediaResult>.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Media upload failed for file {FileName}", fileName);
            return Result<UploadMediaResult>.Fail($"Upload failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteMediaAsync(Guid groupId, Guid mediaId, CancellationToken cancellationToken)
    {
        try
        {
            var media = await db.PostMedia.FirstOrDefaultAsync(m =>
                m.Id == mediaId && m.GroupId == groupId && m.DeletedAt == null, cancellationToken);
            if (media == null)
                return Result<bool>.Fail("Media not found.");

            // Soft delete
            media.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            // TODO: Delete files from storage backend (Supabase, local filesystem, etc.)
            // For now, just mark as deleted in DB

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Media deletion failed for media {MediaId}", mediaId);
            return Result<bool>.Fail("Failed to delete media.");
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetMediaUrlAsync(Guid groupId, Guid mediaId)
    {
        var media = await db.PostMedia
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.GroupId == groupId && m.DeletedAt == null);

        if (media == null)
            return null;

        // Return public URL (would be signed Supabase URL in production)
        return $"/uploads/media/{media.Id}/{media.FileName}";
    }

    private (bool IsValid, string? Error) ValidateFile(Stream stream, string contentType, string mediaType)
    {
        const long maxImageSize = 5 * 1024 * 1024;  // 5 MB
        const long maxVideoSize = 50 * 1024 * 1024; // 50 MB

        // Validate size
        if (mediaType == "image" && stream.Length > maxImageSize)
            return (false, "Image size must be less than 5 MB.");
        if (mediaType == "video" && stream.Length > maxVideoSize)
            return (false, "Video size must be less than 50 MB.");

        // Validate MIME type
        var validImageTypes = new[] { "image/jpeg", "image/png" };
        var validVideoTypes = new[] { "video/mp4" };

        if (mediaType == "image" && !validImageTypes.Contains(contentType))
            return (false, "Only JPEG and PNG images are supported.");
        if (mediaType == "video" && !validVideoTypes.Contains(contentType))
            return (false, "Only MP4 videos are supported.");

        return (true, null);
    }

    private async Task<(string StoragePath, string? ThumbnailPath, MediaMetadata Metadata)> ProcessAndSaveMediaAsync(
        Stream stream, string originalFileName, string contentType, string mediaType, CancellationToken cancellationToken)
    {
        var uniqueId = Guid.NewGuid().ToString();
        var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var safeFileName = $"{uniqueId}{fileExtension}";
        var mediaDir = Path.Combine(_uploadDir, uniqueId);
        Directory.CreateDirectory(mediaDir);

        var metadata = new MediaMetadata();

        if (mediaType == "image")
        {
            // Process image: strip EXIF, resize, compress
            using var image = new Bitmap(stream);
            metadata.Width = image.Width;
            metadata.Height = image.Height;

            // Strip EXIF by creating a new image (this removes all metadata)
            using var newImage = new Bitmap(image.Width, image.Height);
            using var g = Graphics.FromImage(newImage);
            g.DrawImageUnscaled(image, 0, 0);

            // Resize if too large (max 1920px width, maintain aspect ratio)
            Bitmap resizedImage = newImage;
            if (newImage.Width > 1920)
            {
                var newHeight = (int)((newImage.Height * 1920.0) / newImage.Width);
                resizedImage = new Bitmap(newImage, new Size(1920, newHeight));
                metadata.Width = 1920;
                metadata.Height = newHeight;
            }

            // Save as JPEG with compression
            var filePath = Path.Combine(mediaDir, safeFileName);
            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            var qualityParam = new EncoderParameter(Encoder.Quality, 80L);
            var encoderParams = new EncoderParameters(1) { Param = new[] { qualityParam } };
            resizedImage.Save(filePath, jpegEncoder, encoderParams);
            metadata.StoragePath = Path.Combine("media", uniqueId, safeFileName);

            // Create thumbnail
            var thumbSize = new Size(200, 200);
            using var thumbImage = new Bitmap(thumbSize.Width, thumbSize.Height);
            using var thumbGraphics = Graphics.FromImage(thumbImage);
            thumbGraphics.DrawImage(resizedImage, 0, 0, thumbSize.Width, thumbSize.Height);

            var thumbFileName = $"thumb-{safeFileName}";
            var thumbPath = Path.Combine(mediaDir, thumbFileName);
            thumbImage.Save(thumbPath, jpegEncoder, encoderParams);
            metadata.ThumbnailPath = Path.Combine("media", uniqueId, thumbFileName);

            if (resizedImage != newImage)
                resizedImage.Dispose();
        }
        else
        {
            // Video: save as-is (transcoding/thumbnail generation requires FFmpeg)
            // TODO: Integrate FFmpeg.NET or call external FFmpeg binary
            var filePath = Path.Combine(mediaDir, safeFileName);
            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken);

            // Stub: set 0 for duration (would be extracted from video file)
            metadata.DurationSeconds = 0;
            metadata.StoragePath = Path.Combine("media", uniqueId, safeFileName);
        }

        return (metadata.StoragePath!, metadata.ThumbnailPath, metadata);
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        return codecs.FirstOrDefault(c => c.FormatID == format.Guid) ?? codecs[0];
    }

    private class MediaMetadata
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? DurationSeconds { get; set; }
        public string? StoragePath { get; set; }
        public string? ThumbnailPath { get; set; }
    }
}
