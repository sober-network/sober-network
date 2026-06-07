using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

/// <summary>Manages group service-role assignments while enforcing group isolation and visibility rules.</summary>
public class GroupServiceRoleService(AppDbContext db) : IGroupServiceRoleService
{
    /// <inheritdoc />
    public async Task<(IReadOnlyList<GroupServiceRoleResponse>? Roles, string? Error)> GetRolesAsync(
        string groupSlug, Guid requestingUserId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var isMember = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == requestingUserId &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isMember) return (null, "You are not a member of this group.");

        var roles = await db.GroupServiceRoles
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.GroupId == group.Id && r.DeletedAt == null)
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.RoleType)
            .ToListAsync(ct);

        var membershipMap = await db.GroupMemberships
            .AsNoTracking()
            .Where(m =>
                m.GroupId == group.Id &&
                m.DeletedAt == null &&
                m.Status == MemberStatus.Active)
            .ToDictionaryAsync(m => m.UserId, ct);

        var result = roles.Select(r =>
        {
            membershipMap.TryGetValue(r.UserId, out var membership);
            return new GroupServiceRoleResponse(
                r.Id,
                r.UserId,
                r.User.DisplayName,
                membership?.IsEmailShared == true ? r.User.Email : null,
                membership?.IsPhoneShared == true ? r.User.PhoneNumber : null,
                r.RoleType.ToString(),
                r.CustomTitle,
                r.DisplayOrder);
        }).ToList();

        return (result, null);
    }

    /// <inheritdoc />
    public async Task<(GroupServiceRoleResponse? Role, string? Error)> AssignRoleAsync(
        string groupSlug, AssignServiceRoleRequest request, Guid adminUserId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == adminUserId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin) return (null, "You do not have permission to assign service roles.");

        var targetMembership = await db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == request.UserId &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (targetMembership == null) return (null, "Target user is not an active member of this group.");

        if (!Enum.TryParse<GroupServiceRoleType>(request.RoleType, true, out var roleType))
            return (null, "Invalid role type.");

        var role = new GroupServiceRole
        {
            GroupId = group.Id,
            UserId = request.UserId,
            RoleType = roleType,
            CustomTitle = request.CustomTitle,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.GroupServiceRoles.Add(role);
        await db.SaveChangesAsync(ct);

        return (new GroupServiceRoleResponse(
            role.Id,
            role.UserId,
            targetMembership.User.DisplayName,
            targetMembership.IsEmailShared ? targetMembership.User.Email : null,
            targetMembership.IsPhoneShared ? targetMembership.User.PhoneNumber : null,
            role.RoleType.ToString(),
            role.CustomTitle,
            role.DisplayOrder), null);
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> RemoveRoleAsync(
        string groupSlug, Guid roleId, Guid adminUserId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, ct);
        if (group == null) return (false, "Group not found.");

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == adminUserId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin) return (false, "You do not have permission to remove service roles.");

        var role = await db.GroupServiceRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.GroupId == group.Id && r.DeletedAt == null, ct);
        if (role == null) return (false, "Service role not found.");

        role.DeletedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }
}
