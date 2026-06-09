namespace SoberNetwork.Domain.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    
    /// <summary>Reference to uploaded media (replaces external ImageUrl).</summary>
    public Guid? MediaId { get; set; }
    public PostMedia? Media { get; set; }
    
    public string? LinkUrl { get; set; }
    public string? LinkTitle { get; set; }
    public bool IsApproved { get; set; } = false;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public ICollection<PostComment> Comments { get; set; } = [];
}
