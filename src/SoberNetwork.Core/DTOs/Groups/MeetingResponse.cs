namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Full meeting detail returned to authenticated group members. Includes Zoom credentials.</summary>
public record MeetingResponse(
    /// <summary>Unique identifier of the meeting.</summary>
    Guid Id,
    /// <summary>Display name of the meeting.</summary>
    string Name,
    /// <summary>Optional public description of the meeting.</summary>
    string? Description,
    /// <summary>True = weekly recurring. False = one-off.</summary>
    bool IsRecurring,
    /// <summary>DayOfWeek 0=Sun..6=Sat. Null for one-off meetings.</summary>
    int? DayOfWeek,
    /// <summary>Meeting start time in "HH:mm" 24-hour format.</summary>
    string Time,
    /// <summary>Duration in minutes.</summary>
    int DurationMinutes,
    /// <summary>Specific date for one-off meetings.</summary>
    DateTime? OccursOn,
    /// <summary>True = open meeting (anyone welcome). False = closed (AA members only).</summary>
    bool IsOpen,
    /// <summary>Comma-separated meeting formats.</summary>
    string? Formats,
    /// <summary>Meeting language. Null = English.</summary>
    string? Language,
    /// <summary>Physical location address.</summary>
    string? Location,
    /// <summary>Online meeting URL.</summary>
    string? ZoomLink,
    /// <summary>Online meeting identifier.</summary>
    string? ZoomMeetingId,
    /// <summary>Online meeting passcode.</summary>
    string? ZoomPasscode,
    /// <summary>Whether this meeting is currently active.</summary>
    bool IsActive,
    /// <summary>UTC timestamp when the meeting was created.</summary>
    DateTime CreatedAt
);
