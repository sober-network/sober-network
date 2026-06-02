using Microsoft.AspNetCore.Identity;

namespace SoberNetwork.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public DateOnly? SobrietyDate { get; set; }   // opt-in, private by default
    public bool IsSobrietyDatePublic { get; set; } = false;
    public bool IsSuperAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<GroupMembership> GroupMemberships { get; set; } = [];
}
