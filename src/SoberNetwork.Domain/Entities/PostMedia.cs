namespace SoberNetwork.Domain.Entities;

/// <summary>Represents an uploaded media file (image or video) attached to a post.</summary>
public class PostMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>The group that owns this media (tenant isolation).</summary>
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    
    /// <summary>The post this media is attached to (null until post is created).</summary>
    public Guid? PostId { get; set; }
    public Post? Post { get; set; }
    
    /// <summary>The comment this media is attached to (null for post media).</summary>
    public Guid? CommentId { get; set; }
    public PostComment? Comment { get; set; }
    
    /// <summary>Media type: "image" or "video".</summary>
    public string MediaType { get; set; } = string.Empty;  // "image" or "video"
    
    /// <summary>Original filename (sanitized for display).</summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>Storage path in Supabase Storage (e.g., "groups/{groupId}/posts/{postId}/{mediaId}.jpg").</summary>
    public string StoragePath { get; set; } = string.Empty;
    
    /// <summary>MIME type of the file (e.g., "image/jpeg", "video/mp4").</summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>Image width in pixels (images and video thumbnails).</summary>
    public int? ImageWidth { get; set; }
    
    /// <summary>Image height in pixels (images and video thumbnails).</summary>
    public int? ImageHeight { get; set; }
    
    /// <summary>Thumbnail storage path (videos only).</summary>
    public string? ThumbnailPath { get; set; }
    
    /// <summary>Video duration in seconds (videos only).</summary>
    public int? VideoDurationSeconds { get; set; }
    
    /// <summary>When the media was uploaded.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Soft delete timestamp.</summary>
    public DateTime? DeletedAt { get; set; }
}
