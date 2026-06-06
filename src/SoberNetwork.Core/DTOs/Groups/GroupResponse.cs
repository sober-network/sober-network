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
    IReadOnlyList<MeetingResponse> Meetings
);
