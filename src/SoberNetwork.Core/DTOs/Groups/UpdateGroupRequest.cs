namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for updating mutable group fields.</summary>
public record UpdateGroupRequest(
    /// <summary>Optional replacement group name. Maximum 100 characters.</summary>
    string? Name,
    /// <summary>Optional replacement description. Maximum 500 characters.</summary>
    string? Description,
    /// <summary>Optional replacement meeting schedule text. Maximum 1000 characters.</summary>
    string? MeetingSchedule,
    /// <summary>Optional replacement meeting day indicator used by the client.</summary>
    int? MeetingDay,
    /// <summary>Optional replacement meeting start time string. Maximum 5 characters.</summary>
    string? MeetingTime,
    /// <summary>Optional replacement meeting duration in minutes.</summary>
    int? DurationMinutes,
    /// <summary>Optional replacement open-meeting flag.</summary>
    bool? IsOpen,
    /// <summary>Optional replacement language label. Maximum 100 characters.</summary>
    string? Language,
    /// <summary>Optional replacement meeting format description. Maximum 500 characters.</summary>
    string? MeetingFormats,
    /// <summary>Optional replacement online meeting link. Maximum 500 characters.</summary>
    string? ZoomLink,
    /// <summary>Optional replacement online meeting identifier. Maximum 100 characters.</summary>
    string? ZoomMeetingId,
    /// <summary>Optional replacement online meeting passcode. Maximum 100 characters.</summary>
    string? ZoomPasscode,
    /// <summary>Optional replacement time zone label. Maximum 100 characters.</summary>
    string? TimeZone,
    /// <summary>Optional replacement public discoverability flag.</summary>
    bool? IsPublic,
    /// <summary>Optional replacement approval requirement flag.</summary>
    bool? RequiresApproval
);
