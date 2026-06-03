namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Meeting information safe for public/anonymous display.
/// T11/T12 reviewed: meeting time, day, location, open/closed status are non-member data —
/// equivalent to what AA Intergroup publishes publicly. Zoom credentials excluded.
/// </summary>
public record PublicMeetingResponse(
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
    /// <summary>Whether this meeting is currently active.</summary>
    bool IsActive
);
