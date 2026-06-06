using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Meeting record returned by the public meeting finder (cross-group, unauthenticated).
/// T11/T12 reviewed: day, time, name, venue, address, and meeting type are equivalent to what
/// AA Intergroup publishes publicly. Zoom credentials, passcodes, admin Notes, and member data
/// are excluded. PublicJoinUrl is an admin-opted-in URL only (no embedded credentials).
/// </summary>
public record PublicMeetingSearchResponse(
    /// <summary>Unique identifier of the meeting.</summary>
    Guid Id,
    /// <summary>Display name of the meeting.</summary>
    string Name,
    /// <summary>Optional public description.</summary>
    string? Description,
    /// <summary>Host group name.</summary>
    string GroupName,
    /// <summary>Host group slug — used to link to the group public page.</summary>
    string GroupSlug,
    /// <summary>InPerson, Online, or Hybrid.</summary>
    MeetingType MeetingType,
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
    /// <summary>Meeting formats (e.g. Discussion, Speaker, StepStudy).</summary>
    IReadOnlyList<string> Formats,
    /// <summary>Meeting language. Null = English.</summary>
    string? Language,
    /// <summary>Display name of the venue (e.g., "First Presbyterian Church, Community Room").</summary>
    string? VenueName,
    /// <summary>Legacy/combined location string for backwards compatibility.</summary>
    string? Location,
    /// <summary>Street address. Null for online-only meetings.</summary>
    string? Street,
    /// <summary>City. Null for online-only meetings.</summary>
    string? City,
    /// <summary>State or province. Null for online-only meetings.</summary>
    string? State,
    /// <summary>ZIP or postal code.</summary>
    string? PostalCode,
    /// <summary>Country code. Null defaults to US.</summary>
    string? Country,
    /// <summary>GPS latitude. Null if not geocoded.</summary>
    double? Latitude,
    /// <summary>GPS longitude. Null if not geocoded.</summary>
    double? Longitude,
    /// <summary>
    /// Admin-opted-in public join URL (no embedded credentials). Null for in-person meetings
    /// or when the group has not opted to expose a public join URL.
    /// </summary>
    string? PublicJoinUrl,
    /// <summary>Distance in miles from the searched location. Null when no location filter was applied.</summary>
    double? DistanceMiles
);
