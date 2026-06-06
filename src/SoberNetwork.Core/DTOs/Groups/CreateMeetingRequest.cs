using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for creating a new meeting within a group.</summary>
public record CreateMeetingRequest(
    /// <summary>Display name for this meeting. Required, maximum 100 characters.</summary>
    string Name,
    /// <summary>Optional public description. Maximum 1000 characters.</summary>
    string? Description = null,
    /// <summary>Admin-only notes. Maximum 2000 characters.</summary>
    string? Notes = null,
    /// <summary>True = weekly recurring (requires DaysOfWeek). False = one-off (requires OccursOn).</summary>
    bool IsRecurring = true,
    /// <summary>Days of week for recurring meetings (array of 0=Sun..6=Sat). Required when IsRecurring=true.</summary>
    IReadOnlyList<int>? DaysOfWeek = null,
    /// <summary>Meeting start time in "HH:mm" 24-hour format. Required.</summary>
    string Time = "",
    /// <summary>Duration in minutes. Defaults to 60.</summary>
    int DurationMinutes = 60,
    /// <summary>Specific date for one-off meetings. Required when IsRecurring=false.</summary>
    DateTime? OccursOn = null,
    /// <summary>True = open meeting. False = closed.</summary>
    bool IsOpen = true,
    /// <summary>Meeting formats (e.g. Discussion, Speaker, Step Study, Tradition Study, Big Book Study, Literature, Topic, Beginners, Candlelight, Meditation, Birthday / Chip, Men's Meeting, Women's Meeting, Young People's Meeting, LGBTQ+).</summary>
    IReadOnlyList<string>? Formats = null,
    /// <summary>Meeting language. Null = English.</summary>
    string? Language = null,
    /// <summary>InPerson, Online, or Hybrid. Required.</summary>
    MeetingType MeetingType = MeetingType.InPerson,
    /// <summary>Display name of the venue (e.g., "First Presbyterian Church, Community Room").</summary>
    string? VenueName = null,
    /// <summary>Legacy combined location/address string.</summary>
    string? Location = null,
    /// <summary>Street address line 1 for in-person or hybrid meetings.</summary>
    string? Street = null,
    /// <summary>Street address line 2 (apt, suite, etc.) for in-person or hybrid meetings.</summary>
    string? Street2 = null,
    /// <summary>City for in-person or hybrid meetings.</summary>
    string? City = null,
    /// <summary>State or province.</summary>
    string? State = null,
    /// <summary>ZIP or postal code.</summary>
    string? PostalCode = null,
    /// <summary>Country code. Defaults to US.</summary>
    string? Country = null,
    /// <summary>GPS latitude. Populated automatically when address is geocoded.</summary>
    double? Latitude = null,
    /// <summary>GPS longitude. Populated automatically when address is geocoded.</summary>
    double? Longitude = null,
    /// <summary>Online meeting URL — member-visible only (may contain embedded credentials).</summary>
    string? ZoomLink = null,
    /// <summary>Online meeting identifier — member-visible only.</summary>
    string? ZoomMeetingId = null,
    /// <summary>Online meeting passcode — member-visible only, never public.</summary>
    string? ZoomPasscode = null,
    /// <summary>Public join URL for the meeting finder (no embedded credentials).</summary>
    string? PublicJoinUrl = null
);
