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
    /// <summary>Membership status of the current caller (e.g. Active, PendingApproval, Suspended, Banned).</summary>
    string UserMembershipStatus,
    /// <summary>UTC timestamp when the group was created.</summary>
    DateTime CreatedAt,
    /// <summary>Meetings belonging to this group.</summary>
    IReadOnlyList<MeetingResponse> Meetings,
    /// <summary>The next upcoming meeting, if any.</summary>
    NextMeetingDto? NextMeeting,
    /// <summary>Whether the current caller is sharing their phone number with this group.</summary>
    bool UserIsPhoneShared,
    /// <summary>Whether the current caller is sharing their email address with this group.</summary>
    bool UserIsEmailShared,
    /// <summary>Optional district name (e.g. "District 5").</summary>
    string? DistrictName = null,
    /// <summary>Optional area name (e.g. "Area 11").</summary>
    string? AreaName = null,
    /// <summary>Optional state (e.g. "Connecticut").</summary>
    string? State = null,
    /// <summary>Optional district website URL.</summary>
    string? DistrictWebsiteUrl = null,
    /// <summary>Optional area website URL.</summary>
    string? AreaWebsiteUrl = null,
    /// <summary>Optional latitude for district map center.</summary>
    double? DistrictLatitude = null,
    /// <summary>Optional longitude for district map center.</summary>
    double? DistrictLongitude = null
);
