namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Full group detail returned to authenticated members of the group.</summary>
public record GroupResponse(
    /// <summary>Unique identifier of the group.</summary>
    Guid Id,
    /// <summary>Display name of the group.</summary>
    string Name,
    /// <summary>Permanent URL slug for the group.</summary>
    string Slug,
    /// <summary>Optional group description.</summary>
    string? Description,
    /// <summary>Optional human-readable meeting schedule text.</summary>
    string? MeetingSchedule,
    /// <summary>Optional meeting day indicator used by the client.</summary>
    int? MeetingDay,
    /// <summary>Optional meeting start time string.</summary>
    string? MeetingTime,
    /// <summary>Meeting duration in minutes.</summary>
    int DurationMinutes,
    /// <summary>Whether the group is open to newcomers.</summary>
    bool IsOpen,
    /// <summary>Optional meeting language label.</summary>
    string? Language,
    /// <summary>Optional meeting format description.</summary>
    string? MeetingFormats,
    /// <summary>Optional online meeting link for members.</summary>
    string? ZoomLink,
    /// <summary>Optional online meeting identifier for members.</summary>
    string? ZoomMeetingId,
    /// <summary>Optional online meeting passcode for members.</summary>
    string? ZoomPasscode,
    /// <summary>Optional time zone label.</summary>
    string? TimeZone,
    /// <summary>Whether the group is currently active.</summary>
    bool IsActive,
    /// <summary>Whether the group is listed in public discovery views.</summary>
    bool IsPublic,
    /// <summary>Whether join requests require admin approval.</summary>
    bool RequiresApproval,
    /// <summary>Number of active members in the group.</summary>
    int MemberCount,
    /// <summary>Role of the current caller within the group.</summary>
    string UserRole,
    /// <summary>UTC timestamp when the group was created.</summary>
    DateTime CreatedAt
);
