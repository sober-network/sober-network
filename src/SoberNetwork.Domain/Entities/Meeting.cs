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

    /// <summary>Comma-separated meeting formats: Discussion, Speaker, StepStudy, BigBook, Beginners.</summary>
    public string? Formats { get; set; }

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

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Soft delete — meetings are never hard-deleted (T4 audit trail).</summary>
    public DateTime? DeletedAt { get; set; }
}
