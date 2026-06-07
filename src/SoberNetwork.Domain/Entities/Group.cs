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

    // District/Area information for service area display
    public string? DistrictName { get; set; }         // e.g. "District 5"
    public string? AreaName { get; set; }             // e.g. "Area 11"
    public string? State { get; set; }                // e.g. "Connecticut"
    public string? DistrictWebsiteUrl { get; set; }   // e.g. "https://district5ct.org/"
    public string? AreaWebsiteUrl { get; set; }       // e.g. "https://ct-aa.org/"
    public double? DistrictLatitude { get; set; }     // Center of district for map
    public double? DistrictLongitude { get; set; }    // Center of district for map

    public ICollection<GroupMembership> Memberships { get; set; } = [];
    public ICollection<Meeting> Meetings { get; set; } = [];
    public ICollection<GroupServiceRole> ServiceRoles { get; set; } = [];
}
