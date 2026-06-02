using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Enums;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

public class GroupService(
    AppDbContext db,
    IAuditService audit,
    IEmailService email,
    IConfiguration config) : IGroupService
{
    private string AppBaseUrl => config["App:BaseUrl"] ?? "https://app.sobernetwork.group";

    // ── Helpers ────────────────────────────────────────────────────────────────

    private Task<GroupMembership?> GetActiveMembershipAsync(Guid groupId, string userId) =>
        db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == groupId &&
            m.UserId == userId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null);

    private Task<int> CountAdminsAsync(Guid groupId) =>
        db.GroupMemberships.CountAsync(m =>
            m.GroupId == groupId &&
            m.Role == GroupRole.GroupAdmin &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null);

    private static GroupResponse ToGroupResponse(Group g, string userRole, int memberCount) => new(
        g.Id, g.Name, g.Slug, g.Description, g.MeetingSchedule,
        g.ZoomLink, g.TimeZone, g.IsActive, memberCount, userRole, g.CreatedAt);

    private static MemberResponse ToMemberResponse(GroupMembership m) => new(
        m.UserId,
        m.User?.DisplayName ?? string.Empty,
        m.Role.ToString(),
        m.Status.ToString(),
        m.IsProbationary,
        m.JoinedAt,
        m.ApprovedAt);

    private Task<int> GetMemberCountAsync(Guid groupId) =>
        db.GroupMemberships.CountAsync(m =>
            m.GroupId == groupId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null);

    // ── Group queries ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<GroupResponse>> GetUserGroupsAsync(string userId)
    {
        var memberships = await db.GroupMemberships
            .Include(m => m.Group)
            .Where(m =>
                m.UserId == userId &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null &&
                m.Group != null &&
                m.Group.DeletedAt == null)
            .ToListAsync();

        var result = new List<GroupResponse>();
        foreach (var m in memberships)
        {
            var count = await GetMemberCountAsync(m.GroupId);
            result.Add(ToGroupResponse(m.Group!, m.Role.ToString(), count));
        }
        return result;
    }

    public async Task<IReadOnlyList<GroupSummaryResponse>> GetAllGroupsAsync()
    {
        return await db.Groups
            .Where(g => g.DeletedAt == null)
            .Select(g => new GroupSummaryResponse(
                g.Name, g.Slug, g.Description, g.MeetingSchedule, g.TimeZone, g.IsActive))
            .ToListAsync();
    }

    public async Task<GroupResponse?> GetGroupBySlugAsync(string slug, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return null;

        var membership = await GetActiveMembershipAsync(group.Id, userId);
        if (membership == null) return null;  // not a member — access denied (T4)

        var count = await GetMemberCountAsync(group.Id);
        return ToGroupResponse(group, membership.Role.ToString(), count);
    }

    // ── Group mutations ────────────────────────────────────────────────────────

    public async Task<(GroupResponse? Group, string? Error)> CreateGroupAsync(
        CreateGroupRequest request, string creatorUserId)
    {
        if (await db.Groups.AnyAsync(g => g.Slug == request.Slug && g.DeletedAt == null))
            return (null, "A group with that slug already exists.");

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            MeetingSchedule = request.MeetingSchedule,
            ZoomLink = request.ZoomLink,
            TimeZone = request.TimeZone,
            IsActive = true,
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
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupCreated, creatorUserId,
            $"Created group slug={request.Slug}");

        return (ToGroupResponse(group, GroupRole.GroupAdmin.ToString(), 1), null);
    }

    public async Task<(GroupResponse? Group, string? Error)> UpdateGroupAsync(
        string slug, UpdateGroupRequest request, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (null, "Group not found.");

        var membership = await GetActiveMembershipAsync(group.Id, userId);
        if (membership == null || membership.Role != GroupRole.GroupAdmin)
            return (null, "You do not have permission to update this group.");

        if (request.Description != null) group.Description = request.Description;
        if (request.MeetingSchedule != null) group.MeetingSchedule = request.MeetingSchedule;
        if (request.ZoomLink != null) group.ZoomLink = request.ZoomLink;
        if (request.TimeZone != null) group.TimeZone = request.TimeZone;
        group.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await audit.LogAsync(SecurityEventType.GroupUpdated, userId, $"Updated group slug={slug}");

        var count = await GetMemberCountAsync(group.Id);
        return (ToGroupResponse(group, membership.Role.ToString(), count), null);
    }

    public async Task<(bool Success, string? Error)> SoftDeleteGroupAsync(string slug, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (false, "Group not found.");

        var membership = await GetActiveMembershipAsync(group.Id, userId);
        if (membership == null || membership.Role != GroupRole.GroupAdmin)
            return (false, "You do not have permission to delete this group.");

        group.DeletedAt = DateTime.UtcNow;
        group.IsActive = false;
        group.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupDeleted, userId, $"Soft-deleted group slug={slug}");
        return (true, null);
    }

    // ── Member queries ─────────────────────────────────────────────────────────

    public async Task<(PagedResponse<MemberResponse>? Members, string? Error)> GetMembersAsync(
        string slug, string userId, int page = 1, int pageSize = 25)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (null, "Group not found.");

        var callerMembership = await GetActiveMembershipAsync(group.Id, userId);
        if (callerMembership == null) return (null, "You are not a member of this group.");

        var query = db.GroupMemberships
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null)
            .OrderBy(m => m.JoinedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => ToMemberResponse(m))
            .ToListAsync();

        return (new PagedResponse<MemberResponse>(items, page, pageSize, total), null);
    }

    public async Task<(PagedResponse<JoinRequestResponse>? Requests, string? Error)> GetJoinRequestsAsync(
        string slug, string userId, int page = 1, int pageSize = 25)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (null, "Group not found.");

        var callerMembership = await GetActiveMembershipAsync(group.Id, userId);
        if (callerMembership == null || callerMembership.Role != GroupRole.GroupAdmin)
            return (null, "You do not have permission to view join requests.");

        var query = db.GroupMemberships
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.PendingApproval &&
                m.DeletedAt == null)
            .OrderBy(m => m.JoinedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new JoinRequestResponse(m.UserId, m.User!.DisplayName, m.JoinedAt))
            .ToListAsync();

        return (new PagedResponse<JoinRequestResponse>(items, page, pageSize, total), null);
    }

    // ── Membership mutations ───────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> RequestToJoinAsync(string slug, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null && g.IsActive);
        if (group == null) return (false, "Group not found.");

        var existing = await db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == group.Id && m.UserId == userId && m.DeletedAt == null);

        if (existing != null)
        {
            if (existing.Status == MemberStatus.Active) return (false, "You are already a member of this group.");
            if (existing.Status == MemberStatus.PendingApproval) return (true, null);  // idempotent
            if (existing.Status == MemberStatus.Banned) return (false, "You may not join this group.");
        }

        var membership = new GroupMembership
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Member,
            Status = MemberStatus.PendingApproval,
            IsProbationary = true,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.GroupMemberships.Add(membership);
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupJoinRequested, userId,
            $"Requested to join group slug={slug}");

        // Notify all current admins of the new join request (fire-and-forget — don't fail the request)
        var applicant = await db.Users.FindAsync(userId);
        var admins = await db.GroupMemberships
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null)
            .Select(m => m.User!)
            .ToListAsync();

        var approvalLink = $"{AppBaseUrl}/groups/{slug}/join-requests";
        foreach (var admin in admins.Where(a => a.Email != null))
        {
            try { await email.SendGroupJoinRequestAsync(admin.Email!, applicant?.DisplayName ?? "A user", group.Name, approvalLink); }
            catch { /* email failure must not block the join request */ }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ApproveMemberAsync(
        string slug, string targetUserId, string adminUserId)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        if (membership!.Status != MemberStatus.PendingApproval)
            return (false, "This request is not pending approval.");

        membership.Status = MemberStatus.Active;
        membership.ApprovedAt = DateTime.UtcNow;
        membership.ApprovedByUserId = adminUserId;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupMemberApproved, adminUserId,
            $"Approved userId={targetUserId} in group slug={slug}");

        var member = await db.Users.FindAsync(targetUserId);
        if (member?.Email != null)
        {
            try { await email.SendGroupJoinApprovedAsync(member.Email, member.DisplayName, group!.Name); }
            catch { /* email failure must not roll back approval */ }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectMemberAsync(
        string slug, string targetUserId, string adminUserId)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        if (membership!.Status != MemberStatus.PendingApproval)
            return (false, "This request is not pending approval.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupMemberRejected, adminUserId,
            $"Rejected userId={targetUserId} in group slug={slug}");

        var member = await db.Users.FindAsync(targetUserId);
        if (member?.Email != null)
        {
            try { await email.SendGroupJoinRejectedAsync(member.Email, member.DisplayName, group!.Name); }
            catch { }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RemoveMemberAsync(
        string slug, string targetUserId, string adminUserId)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        // Last-admin guard (T2, T9)
        if (membership!.Role == GroupRole.GroupAdmin && await CountAdminsAsync(group!.Id) <= 1)
            return (false, "Cannot remove the last group admin. Assign another admin first.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupMemberRemoved, adminUserId,
            $"Removed userId={targetUserId} from group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LeaveGroupAsync(string slug, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (false, "Group not found.");

        var membership = await GetActiveMembershipAsync(group.Id, userId);
        if (membership == null) return (false, "You are not a member of this group.");

        // Last-admin guard — a group cannot be left without a leader (T2, T9)
        if (membership.Role == GroupRole.GroupAdmin && await CountAdminsAsync(group.Id) <= 1)
            return (false, "You are the only admin. Assign another admin before leaving.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupMemberLeft, userId,
            $"Left group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ClearProbationaryStatusAsync(
        string slug, string targetUserId, string adminUserId)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        if (!membership!.IsProbationary) return (true, null);  // idempotent

        membership.IsProbationary = false;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupProbationCleared, adminUserId,
            $"Cleared probation for userId={targetUserId} in group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeRoleAsync(
        string slug, string targetUserId, string adminUserId, GroupRole newRole)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        // Last-admin guard (T2, T9)
        if (membership!.Role == GroupRole.GroupAdmin &&
            newRole != GroupRole.GroupAdmin &&
            await CountAdminsAsync(group!.Id) <= 1)
        {
            return (false, "Cannot demote the last group admin. Assign another admin first.");
        }

        membership.Role = newRole;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupRoleChanged, adminUserId,
            $"Changed userId={targetUserId} to role={newRole} in group slug={slug}");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeMemberStatusAsync(
        string slug, string targetUserId, string adminUserId, MemberStatus newStatus)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        // Last-admin guard for suspension (T2, T9)
        if (membership!.Role == GroupRole.GroupAdmin &&
            newStatus != MemberStatus.Active &&
            await CountAdminsAsync(group!.Id) <= 1)
        {
            return (false, "Cannot suspend the last group admin. Assign another admin first.");
        }

        membership.Status = newStatus;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync(SecurityEventType.GroupMemberStatusChanged, adminUserId,
            $"Changed userId={targetUserId} to status={newStatus} in group slug={slug}");

        return (true, null);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<(Group? Group, GroupMembership? Membership, string? Error)> GetAdminAndTarget(
        string slug, string targetUserId, string adminUserId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (null, null, "Group not found.");

        var adminMembership = await GetActiveMembershipAsync(group.Id, adminUserId);
        if (adminMembership == null || adminMembership.Role != GroupRole.GroupAdmin)
            return (null, null, "You do not have permission to perform this action.");

        var targetMembership = await db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == group.Id && m.UserId == targetUserId && m.DeletedAt == null);
        if (targetMembership == null) return (null, null, "Member not found.");

        return (group, targetMembership, null);
    }
}
