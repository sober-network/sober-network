using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Enums;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

public class GroupService(AppDbContext db) : IGroupService
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>Returns the active membership row for a user in a group, or null.</summary>
    private Task<GroupMembership?> GetActiveMembershipAsync(Guid groupId, string userId) =>
        db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == groupId &&
            m.UserId == userId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null);

    /// <summary>Counts active GroupAdmins in a group.</summary>
    private Task<int> CountAdminsAsync(Guid groupId) =>
        db.GroupMemberships.CountAsync(m =>
            m.GroupId == groupId &&
            m.Role == GroupRole.GroupAdmin &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null);

    private static GroupResponse ToGroupResponse(Group g, string userRole, int memberCount) => new(
        g.Id,
        g.Name,
        g.Slug,
        g.Description,
        g.MeetingSchedule,
        g.ZoomLink,
        g.TimeZone,
        g.IsActive,
        memberCount,
        userRole,
        g.CreatedAt
    );

    private static MemberResponse ToMemberResponse(GroupMembership m) => new(
        m.UserId,
        m.User?.DisplayName ?? string.Empty,
        m.Role.ToString(),
        m.Status.ToString(),
        m.IsProbationary,
        m.JoinedAt,
        m.ApprovedAt
    );

    private async Task<int> GetMemberCountAsync(Guid groupId) =>
        await db.GroupMemberships.CountAsync(m =>
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
        return (true, null);
    }

    // ── Member queries ─────────────────────────────────────────────────────────

    public async Task<(IReadOnlyList<MemberResponse>? Members, string? Error)> GetMembersAsync(
        string slug, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (null, "Group not found.");

        var callerMembership = await GetActiveMembershipAsync(group.Id, userId);
        if (callerMembership == null) return (null, "You are not a member of this group.");

        var members = await db.GroupMemberships
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null)
            .Select(m => ToMemberResponse(m))
            .ToListAsync();

        return (members, null);
    }

    public async Task<(IReadOnlyList<JoinRequestResponse>? Requests, string? Error)> GetJoinRequestsAsync(
        string slug, string userId)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g =>
            g.Slug == slug && g.DeletedAt == null);
        if (group == null) return (null, "Group not found.");

        var callerMembership = await GetActiveMembershipAsync(group.Id, userId);
        if (callerMembership == null || callerMembership.Role != GroupRole.GroupAdmin)
            return (null, "You do not have permission to view join requests.");

        var requests = await db.GroupMemberships
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.PendingApproval &&
                m.DeletedAt == null)
            .Select(m => new JoinRequestResponse(
                m.UserId,
                m.User!.DisplayName,
                m.JoinedAt))
            .ToListAsync();

        return (requests, null);
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
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
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
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RemoveMemberAsync(
        string slug, string targetUserId, string adminUserId)
    {
        var (group, membership, error) = await GetAdminAndTarget(slug, targetUserId, adminUserId);
        if (error != null) return (false, error);

        // Last-admin guard (T2, T9 — the group must always have a trusted servant)
        if (membership!.Role == GroupRole.GroupAdmin && await CountAdminsAsync(group!.Id) <= 1)
            return (false, "Cannot remove the last group admin. Assign another admin first.");

        membership.DeletedAt = DateTime.UtcNow;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
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
        return (true, null);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Validates that the caller is an admin in the group and the target membership exists.
    /// Returns (group, targetMembership, errorMessage).
    /// </summary>
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
