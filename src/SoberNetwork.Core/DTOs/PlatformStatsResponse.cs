namespace SoberNetwork.Core.DTOs;

/// <summary>
/// Aggregate platform statistics for the public landing page.
/// T11/T12 reviewed: counts only — no names, emails, or identifiable data are exposed.
/// </summary>
public record PlatformStatsResponse(
    int MemberCount,
    int GroupCount,
    int MeetingCount);
