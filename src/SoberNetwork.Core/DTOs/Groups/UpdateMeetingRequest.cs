namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for updating mutable meeting fields.</summary>
public record UpdateMeetingRequest(
    /// <summary>Optional replacement display name. Maximum 100 characters.</summary>
    string? Name = null,
    /// <summary>Optional replacement description. Maximum 1000 characters.</summary>
    string? Description = null,
    /// <summary>Optional replacement admin-only notes. Maximum 2000 characters.</summary>
    string? Notes = null,
    /// <summary>Optional replacement recurring flag.</summary>
    bool? IsRecurring = null,
    /// <summary>Optional replacement day of week 0=Sun..6=Sat.</summary>
    int? DayOfWeek = null,
    /// <summary>Optional replacement start time in "HH:mm" 24-hour format.</summary>
    string? Time = null,
    /// <summary>Optional replacement duration in minutes.</summary>
    int? DurationMinutes = null,
    /// <summary>Optional replacement one-off date.</summary>
    DateTime? OccursOn = null,
    /// <summary>Optional replacement open/closed flag.</summary>
    bool? IsOpen = null,
    /// <summary>Optional replacement formats.</summary>
    IReadOnlyList<string>? Formats = null,
    /// <summary>Optional replacement language.</summary>
    string? Language = null,
    /// <summary>Optional replacement physical location.</summary>
    string? Location = null,
    /// <summary>Optional replacement online meeting URL.</summary>
    string? ZoomLink = null,
    /// <summary>Optional replacement online meeting identifier.</summary>
    string? ZoomMeetingId = null,
    /// <summary>Optional replacement online meeting passcode.</summary>
    string? ZoomPasscode = null,
    /// <summary>Optional replacement active flag.</summary>
    bool? IsActive = null
);
