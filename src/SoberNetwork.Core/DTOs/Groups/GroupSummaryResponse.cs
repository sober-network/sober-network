namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Minimal group info shown in listings — no member data, no Zoom links (T11, T12).
/// Safe to return to any authenticated user browsing available groups.
/// </summary>
public record GroupSummaryResponse(
    string Name,
    string Slug,
    string? Description,
    string? MeetingSchedule,
    string? TimeZone,
    bool IsActive
);
