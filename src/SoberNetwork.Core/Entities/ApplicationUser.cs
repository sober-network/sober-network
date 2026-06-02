using Microsoft.AspNetCore.Identity;

namespace SoberNetwork.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? TimeZone { get; set; }              // IANA timezone, e.g. "America/New_York"
    public DateOnly? SobrietyDate { get; set; }        // opt-in, private by default (T3, T12)
    public bool IsSobrietyDatePublic { get; set; } = false;
    public bool IsDaysSoberPublic { get; set; } = false; // separate toggle: show count without revealing date
    public bool IsSuperAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }       // soft delete — never hard delete a user (T12)
    public DateTime? LastLoginAt { get; set; }

    public ICollection<GroupMembership> GroupMemberships { get; set; } = [];
}
