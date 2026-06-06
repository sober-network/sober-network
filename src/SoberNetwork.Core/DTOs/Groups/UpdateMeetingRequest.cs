using SoberNetwork.Domain.Enums;

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
    /// <summary>Optional replacement days of week 0=Sun..6=Sat.</summary>
    IReadOnlyList<int>? DaysOfWeek = null,
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
    /// <summary>Optional replacement meeting type (InPerson/Online/Hybrid).</summary>
    MeetingType? MeetingType = null,
    /// <summary>Optional replacement venue display name.</summary>
    string? VenueName = null,
    /// <summary>Optional replacement legacy location string.</summary>
    string? Location = null,
    /// <summary>Optional replacement street address.</summary>
    string? Street = null,
    /// <summary>Optional replacement city.</summary>
    string? City = null,
    /// <summary>Optional replacement state/province.</summary>
    string? State = null,
    /// <summary>Optional replacement postal code.</summary>
    string? PostalCode = null,
    /// <summary>Optional replacement country code.</summary>
    string? Country = null,
    /// <summary>Optional replacement latitude.</summary>
    double? Latitude = null,
    /// <summary>Optional replacement longitude.</summary>
    double? Longitude = null,
    /// <summary>Optional replacement online meeting URL (member-visible only).</summary>
    string? ZoomLink = null,
    /// <summary>Optional replacement online meeting identifier.</summary>
    string? ZoomMeetingId = null,
    /// <summary>Optional replacement online meeting passcode.</summary>
    string? ZoomPasscode = null,
    /// <summary>Optional replacement public join URL (no embedded credentials).</summary>
    string? PublicJoinUrl = null,
    /// <summary>Optional replacement active flag.</summary>
    bool? IsActive = null
);
