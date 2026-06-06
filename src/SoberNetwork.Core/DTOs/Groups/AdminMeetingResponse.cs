using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Full meeting detail including admin-only fields. Returned only to GroupAdmin callers.</summary>
public record AdminMeetingResponse(
    /// <summary>Unique identifier of the meeting.</summary>
    Guid Id,
    /// <summary>Display name of the meeting.</summary>
    string Name,
    /// <summary>Optional public description of the meeting.</summary>
    string? Description,
    /// <summary>Admin-only notes (door codes, host info, operational details). Never exposed publicly.</summary>
    string? Notes,
    /// <summary>True = weekly recurring. False = one-off.</summary>
    bool IsRecurring,
    /// <summary>Days of week 0=Sun..6=Sat. Empty array for one-off meetings.</summary>
    IReadOnlyList<int> DaysOfWeek,
    /// <summary>Meeting start time in "HH:mm" 24-hour format.</summary>
    string Time,
    /// <summary>Duration in minutes.</summary>
    int DurationMinutes,
    /// <summary>Specific date for one-off meetings.</summary>
    DateTime? OccursOn,
    /// <summary>True = open meeting (anyone welcome). False = closed (AA members only).</summary>
    bool IsOpen,
    /// <summary>Meeting formats (e.g. Discussion, Speaker, Step Study, Tradition Study, Big Book Study, Literature, Topic, Beginners, Candlelight, Meditation, Birthday / Chip, Men's Meeting, Women's Meeting, Young People's Meeting, LGBTQ+).</summary>
    IReadOnlyList<string> Formats,
    /// <summary>Meeting language. Null = English.</summary>
    string? Language,
    /// <summary>InPerson, Online, or Hybrid.</summary>
    MeetingType MeetingType,
    /// <summary>Display name of the venue.</summary>
    string? VenueName,
    /// <summary>Legacy combined location/address string.</summary>
    string? Location,
    /// <summary>Street address.</summary>
    string? Street,
    /// <summary>City.</summary>
    string? City,
    /// <summary>State or province.</summary>
    string? State,
    /// <summary>ZIP or postal code.</summary>
    string? PostalCode,
    /// <summary>Country code.</summary>
    string? Country,
    /// <summary>GPS latitude.</summary>
    double? Latitude,
    /// <summary>GPS longitude.</summary>
    double? Longitude,
    /// <summary>Online meeting URL (member-visible only).</summary>
    string? ZoomLink,
    /// <summary>Online meeting identifier (member-visible only).</summary>
    string? ZoomMeetingId,
    /// <summary>Online meeting passcode (member-visible only).</summary>
    string? ZoomPasscode,
    /// <summary>Public join URL (no embedded credentials).</summary>
    string? PublicJoinUrl,
    /// <summary>Whether this meeting is currently active.</summary>
    bool IsActive,
    /// <summary>UTC timestamp when the meeting was created.</summary>
    DateTime CreatedAt,
    /// <summary>UTC timestamp of the last update.</summary>
    DateTime UpdatedAt
);
