using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

/// <summary>
/// Returns aggregate platform statistics for public display.
/// T11/T12 reviewed: returns integer counts only — no names, emails, or
/// identifiable data are queried or returned.
/// </summary>
public class StatsService(AppDbContext db) : IStatsService
{
    public async Task<PlatformStatsResponse> GetPlatformStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var memberCount = await db.GroupMemberships
            .AsNoTracking()
            .Where(m => m.Status == MemberStatus.Active && m.DeletedAt == null)
            .Select(m => m.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var groupCount = await db.Groups
            .AsNoTracking()
            .CountAsync(g => g.DeletedAt == null, cancellationToken);

        var meetingCount = await db.Meetings
            .AsNoTracking()
            .Where(m => m.DeletedAt == null && m.Group.DeletedAt == null)
            .CountAsync(cancellationToken);

        return new PlatformStatsResponse(memberCount, groupCount, meetingCount);
    }
}
