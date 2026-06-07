using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Options;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

public class GroupService(
    AppDbContext db,
    IAuditService audit,
    IEmailService email,
    IOptions<AppOptions> appOptions) : IGroupService
{
    private string AppBaseUrl => appOptions.Value.BaseUrl;

    // ── Helpers ────────────────────────────────────────────────────────────────

    private Task<GroupMembership?> GetActiveMembershipAsync(Guid groupId, Guid userId, CancellationToken ct = default) =>
        db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == groupId &&
            m.UserId == userId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);

    private Task<int> CountAdminsAsync(Guid groupId, CancellationToken ct = default) =>
        db.GroupMemberships.CountAsync(m =>
            m.GroupId == groupId &&
            m.Role == GroupRole.GroupAdmin &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static MeetingResponse ToMeetingResponse(Meeting m) => new(
        m.Id, m.Name, m.Description, m.IsRecurring, m.DaysOfWeek ?? new int[] { }, m.Time,
        m.DurationMinutes, m.OccursOn, m.IsOpen, m.Formats, m.Language,
        m.MeetingType, m.VenueName, m.Location, m.Street, m.City, m.State,
        m.PostalCode, m.Country, m.Latitude, m.Longitude,
        m.ZoomLink, m.ZoomMeetingId, m.ZoomPasscode, m.PublicJoinUrl, m.IsActive, m.CreatedAt);

    private static PublicMeetingResponse ToPublicMeetingResponse(Meeting m) => new(
        m.Id, m.Name, m.Description, m.IsRecurring, m.DaysOfWeek ?? new int[] { }, m.Time,
        m.DurationMinutes, m.OccursOn, m.IsOpen, m.Formats, m.Language,
        m.MeetingType, m.VenueName, m.Location, m.Street, m.City, m.State,
        m.PostalCode, m.Country, m.Latitude, m.Longitude, m.PublicJoinUrl);

    private static IReadOnlyList<MeetingResponse> ActiveMeetings(Group g) =>
        g.Meetings
            .Where(m => m.DeletedAt == null && m.IsActive)
            .OrderBy(m => m.IsRecurring ? 0 : 1)
            .ThenBy(m => m.DaysOfWeek != null && m.DaysOfWeek.Length > 0 ? m.DaysOfWeek[0] : int.MaxValue)
            .ThenBy(m => m.Time)
            .Select(ToMeetingResponse).ToList();

    private static IReadOnlyList<PublicMeetingResponse> ActivePublicMeetings(Group g) =>
        g.Meetings
            .Where(m => m.DeletedAt == null && m.IsActive)
            .OrderBy(m => m.IsRecurring ? 0 : 1)
            .ThenBy(m => m.DaysOfWeek != null && m.DaysOfWeek.Length > 0 ? m.DaysOfWeek[0] : int.MaxValue)
            .ThenBy(m => m.Time)
            .Select(ToPublicMeetingResponse).ToList();

    private static GroupResponse ToGroupResponse(Group g, string userRole, string userMembershipStatus, int memberCount) => new(
        g.Id, g.Name, g.Slug, g.Description, g.TimeZone,
        g.IsActive, g.IsPublic, g.RequiresApproval,
        memberCount, userRole, userMembershipStatus, g.CreatedAt, ActiveMeetings(g));

    private static GroupSummaryResponse ToGroupSummaryResponse(Group group) => new(
        group.Name, group.Slug, group.Description, group.TimeZone,
        group.IsActive, group.IsPublic, group.RequiresApproval,
        ActivePublicMeetings(group));

    private static MemberResponse ToMemberResponse(GroupMembership m) => new(
        m.UserId,
        m.User?.DisplayName ?? string.Empty,
        m.Role.ToString(),
        m.Status.ToString(),
        m.IsProbationary,
        m.JoinedAt,
        m.ApprovedAt);

    private Task<int> GetMemberCountAsync(Guid groupId, CancellationToken ct = default) =>
        db.GroupMemberships.CountAsync(m =>
            m.GroupId == groupId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);

    // ── Group queries ──────────────────────────────────────────────────────────

    public async Task<(PagedResponse<GroupResponse>? Groups, string? Error)> GetUserGroupsAsync(Guid userId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        // Build filter separately so we can reuse it without consuming the IQueryable
        var filterQuery = db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.Group)
                .ThenInclude(g => g!.Meetings)
            .Where(m =>
                m.UserId == userId &&
                (m.Status == MemberStatus.Active || m.Status == MemberStatus.PendingApproval) &&
                m.DeletedAt == null &&
                m.Group != null &&
                m.Group.DeletedAt == null);

        // Get total count from fresh query (can't reuse after CountAsync)
        var totalCount = await filterQuery.CountAsync(ct);

        // Get paginated data from separate fresh query
        var memberships = await db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.Group)
                .ThenInclude(g => g!.Meetings)
            .Where(m =>
                m.UserId == userId &&
                (m.Status == MemberStatus.Active || m.Status == MemberStatus.PendingApproval) &&
                m.DeletedAt == null &&
                m.Group != null &&
                m.Group.DeletedAt == null)
            .OrderBy(m => m.Group!.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var groupIds = memberships.Select(m => m.GroupId).ToList();
        var counts = await db.GroupMemberships
            .AsNoTracking()
            .Where(x =>
                groupIds.Contains(x.GroupId) &&
                x.Status == MemberStatus.Active &&
                x.DeletedAt == null)
            .GroupBy(x => x.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.GroupId, g => g.Count, ct);

        var items = memberships
            .Select(m => ToGroupResponse(
                m.Group!,
                m.Role.ToString(),
                m.Status.ToString(),
                counts.GetValueOrDefault(m.GroupId)))
            .ToList();

        return (new PagedResponse<GroupResponse>(items, page, pageSize, totalCount), null);
    }

    public async Task<(PagedResponse<GroupSummaryResponse>? Groups, string? Error)> GetAllGroupsAsync(int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        // Get total count from fresh query (can't reuse after CountAsync)
        var totalCount = await db.Groups
            .AsNoTracking()
            .Where(g => g.DeletedAt == null)
            .CountAsync(ct);

        // Get paginated data from separate fresh query
        var groups = await db.Groups
            .AsNoTracking()
            .Include(g => g.Meetings)
            .Where(g => g.DeletedAt == null)
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = groups.Select(ToGroupSummaryResponse).ToList();
        return (new PagedResponse<GroupSummaryResponse>(items, page, pageSize, totalCount), null);
    }

    public async Task<GroupSummaryResponse?> GetGroupInfoAsync(string slug, CancellationToken ct = default)
    {
        // IsPublic controls group-list discoverability, not direct-link access (T4 — group autonomy).
        // A group admin can share a join link for a private group; the recipient can still see info + request to join.
        var group = await db.Groups
            .AsNoTracking()
            .Include(g => g.Meetings)
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null && g.IsActive, ct);
        if (group == null) return null;
        return ToGroupSummaryResponse(group);
    }

    public async Task<GroupResponse?> GetGroupBySlugAsync(string slug, Guid userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .Include(g => g.Meetings)
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return null;

        var membership = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.DeletedAt == null, ct);
        if (membership == null) return null;  // not a member — access denied (T4)

        var count = await db.GroupMemberships
            .AsNoTracking()
            .CountAsync(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        return ToGroupResponse(group, membership.Role.ToString(), membership.Status.ToString(), count);
    }

    // ── Group mutations ────────────────────────────────────────────────────────

    public async Task<(GroupResponse? Group, string? Error)> CreateGroupAsync(
        CreateGroupRequest request, Guid creatorUserId, CancellationToken ct = default)
    {
        if (await db.Groups.AnyAsync(g => g.Slug == request.Slug && g.DeletedAt == null, ct))
            return (null, "A group with that slug already exists.");

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            Description = NullIfWhiteSpace(request.Description),
            TimeZone = NullIfWhiteSpace(request.TimeZone),
            IsPublic = request.IsPublic,
            RequiresApproval = request.RequiresApproval,
            IsActive = true,
            Meetings = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Creator automatically becomes GroupAdmin (T9 — trusted servant)
        var membership = new GroupMembership
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = creatorUserId,
            Role = GroupRole.GroupAdmin,
            Status = MemberStatus.Active,
            IsProbationary = false,
            JoinedAt = DateTime.UtcNow,
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Groups.Add(group);
        db.GroupMemberships.Add(membership);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupCreated, creatorUserId,
            $"Created group slug={request.Slug}");

        return (ToGroupResponse(group, GroupRole.GroupAdmin.ToString(), MemberStatus.Active.ToString(), 1), null);
    }

    public async Task<(GroupResponse? Group, string? Error)> UpdateGroupAsync(
        string slug, UpdateGroupRequest request, Guid userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .Include(g => g.Meetings)
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var membership = await GetActiveMembershipAsync(group.Id, userId, ct);
        if (membership == null || membership.Role != GroupRole.GroupAdmin)
            return (null, "You do not have permission to update this group.");

        if (request.Name != null) group.Name = request.Name.Trim();
        if (request.Description != null) group.Description = NullIfWhiteSpace(request.Description);
        if (request.TimeZone != null) group.TimeZone = NullIfWhiteSpace(request.TimeZone);
        if (request.IsPublic != null) group.IsPublic = request.IsPublic.Value;
        if (request.RequiresApproval != null) group.RequiresApproval = request.RequiresApproval.Value;
        group.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await audit.LogAsync(SecurityEventType.GroupUpdated, userId, $"Updated group slug={slug}");

        var count = await GetMemberCountAsync(group.Id, ct);
        return (ToGroupResponse(group, membership.Role.ToString(), membership.Status.ToString(), count), null);
    }

    public async Task<(bool Success, string? Error)> SoftDeleteGroupAsync(string slug, Guid userId, CancellationToken ct = default)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (false, "Group not found.");

        var membership = await GetActiveMembershipAsync(group.Id, userId, ct);
        if (membership == null || membership.Role != GroupRole.GroupAdmin)
            return (false, "You do not have permission to delete this group.");

        group.DeletedAt = DateTime.UtcNow;
        group.IsActive = false;
        group.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupDeleted, userId, $"Soft-deleted group slug={slug}");
        return (true, null);
    }

    // ── Member queries ─────────────────────────────────────────────────────────

    public async Task<(PagedResponse<MemberResponse>? Members, string? Error)> GetMembersAsync(
        string slug, Guid userId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var callerMembership = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (callerMembership == null) return (null, "You are not a member of this group.");

        var baseQuery = db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null)
            .OrderBy(m => m.JoinedAt);

        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => ToMemberResponse(m))
            .ToListAsync(ct);

        return (new PagedResponse<MemberResponse>(items, page, pageSize, total), null);
    }

    public async Task<(PagedResponse<JoinRequestResponse>? Requests, string? Error)> GetJoinRequestsAsync(
        string slug, Guid userId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var callerMembership = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (callerMembership == null || callerMembership.Role != GroupRole.GroupAdmin)
            return (null, "You do not have permission to view join requests.");

        var baseQuery = db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.PendingApproval &&
                m.DeletedAt == null)
            .OrderBy(m => m.JoinedAt);

        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new JoinRequestResponse(m.UserId, m.User!.DisplayName, m.JoinedAt))
            .ToListAsync(ct);

        return (new PagedResponse<JoinRequestResponse>(items, page, pageSize, total), null);
    }

    // ── Membership mutations ───────────────────────────────────────────────────

    public async Task<(bool Success, bool AutoApproved, string? Error)> RequestToJoinAsync(string slug, Guid userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null && g.IsActive, ct);
        if (group == null) return (false, false, "Group not found.");

        var existing = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.GroupId == group.Id && m.UserId == userId && m.DeletedAt == null, ct);

        if (existing != null)
        {
            if (existing.Status == MemberStatus.Active) return (false, false, "You are already a member of this group.");
            if (existing.Status == MemberStatus.PendingApproval) return (true, false, null);  // idempotent
            if (existing.Status == MemberStatus.Banned) return (false, false, "You may not join this group.");
        }

        var autoApprove = !group.RequiresApproval;
        var membership = new GroupMembership
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Member,
            Status = autoApprove ? MemberStatus.Active : MemberStatus.PendingApproval,
            IsProbationary = true,
            JoinedAt = DateTime.UtcNow,
            ApprovedAt = autoApprove ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.GroupMemberships.Add(membership);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(
            autoApprove ? SecurityEventType.GroupMemberApproved : SecurityEventType.GroupJoinRequested,
            userId,
            autoApprove
                ? $"Auto-approved join for group slug={slug}"
                : $"Requested to join group slug={slug}");

        if (autoApprove)
        {
            return (true, true, null);
        }

        // Notify all current admins of the new join request (fire-and-forget — don't fail the request)
        var applicant = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        var admins = await db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null)
            .Select(m => m.User!)
            .ToListAsync(ct);

        var approvalLink = $"{AppBaseUrl}/groups/{slug}/join-requests";
        foreach (var admin in admins.Where(a => a.Email != null))
        {
            try { await email.SendGroupJoinRequestAsync(admin.Email!, applicant?.DisplayName ?? "A user", group.Name, approvalLink); }
            catch { /* email failure must not block the join request */ }
        }

        return (true, false, null);
    }

    public async Task<(bool Success, string? Error)> ApproveMemberAsync(
        string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId, ct);
        if (error != null) return (false, error);

        if (membership!.Status != MemberStatus.PendingApproval)
            return (false, "This request is not pending approval.");

        membership.Status = MemberStatus.Active;
        membership.ApprovedAt = DateTime.UtcNow;
        membership.ApprovedByUserId = adminUserId;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupMemberApproved, adminUserId,
            $"Approved userId={targetUserId} in group slug={slug}");

        var member = await db.Users.FindAsync(new object[] { targetUserId }, ct);
        if (member?.Email != null)
        {
            try { await email.SendGroupJoinApprovedAsync(member.Email, member.DisplayName, group!.Name); }
            catch { /* email failure must not roll back approval */ }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectMemberAsync(
        string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId, ct);
        if (error != null) return (false, error);

        if (membership!.Status != MemberStatus.PendingApproval)
            return (false, "This request is not pending approval.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupMemberRejected, adminUserId,
            $"Rejected userId={targetUserId} in group slug={slug}");

        var member = await db.Users.FindAsync(new object[] { targetUserId }, ct);
        if (member?.Email != null)
        {
            try { await email.SendGroupJoinRejectedAsync(member.Email, member.DisplayName, group!.Name); }
            catch { }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RemoveMemberAsync(
        string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId, ct);
        if (error != null) return (false, error);

        // Last-admin guard (T2, T9)
        if (membership!.Role == GroupRole.GroupAdmin && await CountAdminsAsync(group!.Id, ct) <= 1)
            return (false, "Cannot remove the last group admin. Assign another admin first.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupMemberRemoved, adminUserId,
            $"Removed userId={targetUserId} from group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LeaveGroupAsync(string slug, Guid userId, CancellationToken ct = default)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (false, "Group not found.");

        var membership = await GetActiveMembershipAsync(group.Id, userId, ct);
        if (membership == null) return (false, "You are not a member of this group.");

        // Last-admin guard — a group cannot be left without a leader (T2, T9)
        if (membership.Role == GroupRole.GroupAdmin && await CountAdminsAsync(group.Id, ct) <= 1)
            return (false, "You are the only admin. Assign another admin before leaving.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupMemberLeft, userId,
            $"Left group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ClearProbationaryStatusAsync(
        string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId, ct);
        if (error != null) return (false, error);

        if (!membership!.IsProbationary) return (true, null);  // idempotent

        membership.IsProbationary = false;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupProbationCleared, adminUserId,
            $"Cleared probation for userId={targetUserId} in group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeRoleAsync(
        string slug, Guid targetUserId, Guid adminUserId, GroupRole newRole, CancellationToken ct = default)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId, ct);
        if (error != null) return (false, error);

        // Last-admin guard (T2, T9)
        if (membership!.Role == GroupRole.GroupAdmin &&
            newRole != GroupRole.GroupAdmin &&
            await CountAdminsAsync(group!.Id, ct) <= 1)
        {
            return (false, "Cannot demote the last group admin. Assign another admin first.");
        }

        membership.Role = newRole;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupRoleChanged, adminUserId,
            $"Changed userId={targetUserId} to role={newRole} in group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeMemberStatusAsync(
        string slug, Guid targetUserId, Guid adminUserId, MemberStatus newStatus, CancellationToken ct = default)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId, ct);
        if (error != null) return (false, error);

        // Last-admin guard for suspension (T2, T9)
        if (membership!.Role == GroupRole.GroupAdmin &&
            newStatus != MemberStatus.Active &&
            await CountAdminsAsync(group!.Id, ct) <= 1)
        {
            return (false, "Cannot suspend the last group admin. Assign another admin first.");
        }

        membership.Status = newStatus;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.GroupMemberStatusChanged, adminUserId,
            $"Changed userId={targetUserId} to status={newStatus} in group slug={slug}");

        return (true, null);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<(Group? Group, GroupMembership? Membership, string? Error)> GetAdminAndTarget(
        string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, null, "Group not found.");

        var adminMembership = await GetActiveMembershipAsync(group.Id, adminUserId, ct);
        if (adminMembership == null || adminMembership.Role != GroupRole.GroupAdmin)
            return (null, null, "You do not have permission to perform this action.");

        var targetMembership = await db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == group.Id && m.UserId == targetUserId && m.DeletedAt == null, ct);
        if (targetMembership == null) return (null, null, "Member not found.");

        return (group, targetMembership, null);
    }
}
