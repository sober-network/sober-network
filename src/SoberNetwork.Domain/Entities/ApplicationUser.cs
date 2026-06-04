using Microsoft.AspNetCore.Identity;

namespace SoberNetwork.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? TimeZone { get; set; }              // IANA timezone, e.g. "America/New_York"
    public DateOnly? SobrietyDate { get; set; }        // opt-in, private by default (T3, T12)
    public bool IsSobrietyDatePublic { get; set; } = false;
    public bool IsSuperAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }       // soft delete — never hard delete a user (T12)
    public DateTime? LastLoginAt { get; set; }

    // ── Mailing address (opt-in, T3) ─────────────────────────────────────────
    // Used for mailing sobriety chips and as the default "near me" starting point
    // in the meeting finder. Never exposed publicly (T12).

    public string? MailingStreet { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountry { get; set; }
    /// <summary>Cached latitude from geocoded mailing address. Used for fast meeting-finder distance queries.</summary>
    public double? MailingLatitude { get; set; }
    /// <summary>Cached longitude from geocoded mailing address. Used for fast meeting-finder distance queries.</summary>
    public double? MailingLongitude { get; set; }

    public ICollection<GroupMembership> GroupMemberships { get; set; } = [];
}
