using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Manages AA service role assignments within groups.</summary>
public interface IGroupServiceRoleService
{
    /// <summary>Returns all active service roles for a group. Caller must be an active member.</summary>
    Task<(IReadOnlyList<GroupServiceRoleResponse>? Roles, string? Error)> GetRolesAsync(
        string groupSlug, Guid requestingUserId, CancellationToken ct = default);

    /// <summary>Assigns a service role to a member. Caller must be a GroupAdmin.</summary>
    Task<(GroupServiceRoleResponse? Role, string? Error)> AssignRoleAsync(
        string groupSlug, AssignServiceRoleRequest request, Guid adminUserId, CancellationToken ct = default);

    /// <summary>Removes a service role assignment. Caller must be a GroupAdmin.</summary>
    Task<(bool Success, string? Error)> RemoveRoleAsync(
        string groupSlug, Guid roleId, Guid adminUserId, CancellationToken ct = default);
}
