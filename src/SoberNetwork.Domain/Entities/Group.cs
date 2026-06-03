namespace SoberNetwork.Domain.Entities;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // e.g. "earlybird" → earlybird.sobernetwork.group
    public string? Description { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }           // soft delete — groups are never permanently removed (T4)
    public bool IsPublic { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;

    public ICollection<GroupMembership> Memberships { get; set; } = [];
    public ICollection<Meeting> Meetings { get; set; } = [];
}
