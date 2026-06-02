namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Full group detail — returned to authenticated members of the group.</summary>
public record GroupResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? MeetingSchedule,
    string? ZoomLink,          // authenticated members only — not public (T12)
    string? TimeZone,
    bool IsActive,
    int MemberCount,
    string UserRole,           // the requesting user's role in this group
    DateTime CreatedAt
);
