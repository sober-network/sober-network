namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for creating a new meeting within a group.</summary>
public record CreateMeetingRequest(
    /// <summary>Display name for this meeting. Required, maximum 100 characters.</summary>
    string Name,
    /// <summary>Optional public description. Maximum 1000 characters.</summary>
    string? Description = null,
    /// <summary>Admin-only notes. Maximum 2000 characters.</summary>
    string? Notes = null,
    /// <summary>True = weekly recurring (requires DayOfWeek). False = one-off (requires OccursOn).</summary>
    bool IsRecurring = true,
    /// <summary>Day of week 0=Sun..6=Sat. Required when IsRecurring=true.</summary>
    int? DayOfWeek = null,
    /// <summary>Meeting start time in "HH:mm" 24-hour format. Required.</summary>
    string Time = "",
    /// <summary>Duration in minutes. Defaults to 60.</summary>
    int DurationMinutes = 60,
    /// <summary>Specific date for one-off meetings. Required when IsRecurring=false.</summary>
    DateTime? OccursOn = null,
    /// <summary>True = open meeting. False = closed.</summary>
    bool IsOpen = true,
    /// <summary>Comma-separated meeting formats.</summary>
    string? Formats = null,
    /// <summary>Meeting language. Null = English.</summary>
    string? Language = null,
    /// <summary>Physical location address.</summary>
    string? Location = null,
    /// <summary>Online meeting URL.</summary>
    string? ZoomLink = null,
    /// <summary>Online meeting identifier.</summary>
    string? ZoomMeetingId = null,
    /// <summary>Online meeting passcode.</summary>
    string? ZoomPasscode = null
);
