using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Domain.Entities;

public class Meeting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    /// <summary>Display name for this meeting, e.g. "Monday Morning Step Study".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional public description of the meeting.</summary>
    public string? Description { get; set; }

    /// <summary>Admin-only notes (door codes, host info, operational details).</summary>
    public string? Notes { get; set; }

    /// <summary>True = weekly recurring. False = one-off.</summary>
    public bool IsRecurring { get; set; } = true;

    /// <summary>DayOfWeek 0=Sun..6=Sat. Required when IsRecurring=true, must be null for one-off.</summary>
    public int? DayOfWeek { get; set; }

    /// <summary>Meeting start time in "HH:mm" 24-hour format.</summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>Duration in minutes.</summary>
    public int DurationMinutes { get; set; } = 60;

    /// <summary>Specific date for one-off meetings. Must be null when IsRecurring=true.</summary>
    public DateTime? OccursOn { get; set; }

    /// <summary>True = open meeting (anyone welcome). False = closed (AA members only).</summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>Meeting formats, e.g. Discussion, Speaker, StepStudy, BigBook, Beginners.</summary>
    public List<string> Formats { get; set; } = [];

    /// <summary>Meeting language. Null defaults to English.</summary>
    public string? Language { get; set; }

    /// <summary>Physical location address for in-person or hybrid meetings.</summary>
    public string? Location { get; set; }

    /// <summary>Online meeting URL for Zoom or similar.</summary>
    public string? ZoomLink { get; set; }

    /// <summary>Online meeting identifier (e.g. Zoom Meeting ID).</summary>
    public string? ZoomMeetingId { get; set; }

    /// <summary>Online meeting passcode — visible to authenticated members only, never public.</summary>
    public string? ZoomPasscode { get; set; }

    /// <summary>
    /// Explicitly public join URL for the meeting finder (e.g. a waiting-room Zoom link with no embedded passcode).
    /// Distinct from ZoomLink which may contain embedded credentials. T11/T12 reviewed: admin opts in per meeting.
    /// </summary>
    public string? PublicJoinUrl { get; set; }

    // ── Meeting type ─────────────────────────────────────────────────────────

    /// <summary>InPerson, Online, or Hybrid.</summary>
    public MeetingType MeetingType { get; set; } = MeetingType.InPerson;

    // ── Structured address fields (for geocoding + directions) ───────────────

    /// <summary>Display name of the venue (e.g., "First Presbyterian Church, Community Room").</summary>
    public string? VenueName { get; set; }

    /// <summary>Street address of the meeting location.</summary>
    public string? Street { get; set; }

    /// <summary>City of the meeting location.</summary>
    public string? City { get; set; }

    /// <summary>State or province of the meeting location.</summary>
    public string? State { get; set; }

    /// <summary>ZIP or postal code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Country code. Defaults to "US".</summary>
    public string? Country { get; set; }

    /// <summary>GPS latitude — populated via geocoding or manual entry. Used for distance-based search.</summary>
    public double? Latitude { get; set; }

    /// <summary>GPS longitude — populated via geocoding or manual entry. Used for distance-based search.</summary>
    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Soft delete — meetings are never hard-deleted (T4 audit trail).</summary>
    public DateTime? DeletedAt { get; set; }
}
