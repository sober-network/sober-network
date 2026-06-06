using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Meeting information safe for public/anonymous display (per-group page).
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
    /// <summary>Meeting formats (e.g. Discussion, Speaker, StepStudy, BigBook, Beginners).</summary>
    IReadOnlyList<string> Formats,
    /// <summary>Meeting language. Null = English.</summary>
    string? Language,
    /// <summary>InPerson, Online, or Hybrid.</summary>
    MeetingType MeetingType,
    /// <summary>Display name of the venue.</summary>
    string? VenueName,
    /// <summary>Legacy combined location/address string.</summary>
    string? Location,
    /// <summary>Street address. Null for online-only meetings.</summary>
    string? Street,
    /// <summary>City. Null for online-only meetings.</summary>
    string? City,
    /// <summary>State or province.</summary>
    string? State,
    /// <summary>ZIP or postal code.</summary>
    string? PostalCode,
    /// <summary>Country code.</summary>
    string? Country,
    /// <summary>GPS latitude. Null if not geocoded.</summary>
    double? Latitude,
    /// <summary>GPS longitude. Null if not geocoded.</summary>
    double? Longitude,
    /// <summary>Public join URL (no embedded credentials). Null unless admin opted in.</summary>
    string? PublicJoinUrl
);
