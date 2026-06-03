namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for creating a new group.</summary>
public record CreateGroupRequest(
    /// <summary>Group name shown throughout the application. Required, maximum 100 characters.</summary>
    string Name,
    /// <summary>Permanent URL slug. Required, lowercase letters, numbers, and hyphens only, maximum 50 characters.</summary>
    string Slug,
    /// <summary>Optional group description. Maximum 500 characters.</summary>
    string? Description = null,
    /// <summary>Optional human-readable meeting schedule text. Maximum 1000 characters.</summary>
    string? MeetingSchedule = null,
    /// <summary>Optional meeting day indicator used by the client.</summary>
    int? MeetingDay = null,
    /// <summary>Optional meeting start time string. Maximum 5 characters.</summary>
    string? MeetingTime = null,
    /// <summary>Meeting duration in minutes. Defaults to 60.</summary>
    int DurationMinutes = 60,
    /// <summary>Whether the group is open to newcomers.</summary>
    bool IsOpen = true,
    /// <summary>Optional meeting language label. Maximum 100 characters.</summary>
    string? Language = null,
    /// <summary>Optional meeting format description. Maximum 500 characters.</summary>
    string? MeetingFormats = null,
    /// <summary>Optional online meeting link. Maximum 500 characters.</summary>
    string? ZoomLink = null,
    /// <summary>Optional online meeting identifier. Maximum 100 characters.</summary>
    string? ZoomMeetingId = null,
    /// <summary>Optional online meeting passcode. Maximum 100 characters.</summary>
    string? ZoomPasscode = null,
    /// <summary>Optional time zone label. Maximum 100 characters.</summary>
    string? TimeZone = null,
    /// <summary>Whether the group is listed in public discovery views.</summary>
    bool IsPublic = true,
    /// <summary>Whether join requests require approval from a group admin.</summary>
    bool RequiresApproval = true
);
