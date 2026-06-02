using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Enums;

namespace SoberNetwork.Core.Interfaces;

public interface IGroupService
{
    // ── Group queries ──────────────────────────────────────────────────────────

    /// <summary>Returns all groups the user is an active member of.</summary>
    Task<IReadOnlyList<GroupResponse>> GetUserGroupsAsync(string userId);

    /// <summary>Returns a summary listing of all active groups. SuperAdmin only.</summary>
    Task<IReadOnlyList<GroupSummaryResponse>> GetAllGroupsAsync();

    /// <summary>
    /// Returns full group detail. Caller must be an active member of the group (T4).
    /// Returns null if the group does not exist or the caller is not a member.
    /// </summary>
    Task<GroupResponse?> GetGroupBySlugAsync(string slug, string userId);

    // ── Group mutations ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new group and automatically makes the creator a GroupAdmin (T9 — trusted servants).
    /// Returns the created group or an error message.
    /// </summary>
    Task<(GroupResponse? Group, string? Error)> CreateGroupAsync(CreateGroupRequest request, string creatorUserId);

    /// <summary>
    /// Updates mutable fields. Caller must be a GroupAdmin.
    /// Returns the updated group or an error message.
    /// </summary>
    Task<(GroupResponse? Group, string? Error)> UpdateGroupAsync(string slug, UpdateGroupRequest request, string userId);

    /// <summary>
    /// Soft-deletes the group. Caller must be a GroupAdmin.
    /// Returns success flag and optional error message.
    /// </summary>
    Task<(bool Success, string? Error)> SoftDeleteGroupAsync(string slug, string userId);

    // ── Member queries ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns active members of the group. Caller must be an active member (T4, T12).
    /// </summary>
    Task<(IReadOnlyList<MemberResponse>? Members, string? Error)> GetMembersAsync(string slug, string userId);

    /// <summary>
    /// Returns pending join requests. Caller must be a GroupAdmin.
    /// </summary>
    Task<(IReadOnlyList<JoinRequestResponse>? Requests, string? Error)> GetJoinRequestsAsync(string slug, string userId);

    // ── Membership mutations ───────────────────────────────────────────────────

    /// <summary>
    /// Submits a join request for the caller. Idempotent — returns existing request if already pending.
    /// </summary>
    Task<(bool Success, string? Error)> RequestToJoinAsync(string slug, string userId);

    /// <summary>
    /// Approves a pending join request. Caller must be a GroupAdmin.
    /// </summary>
    Task<(bool Success, string? Error)> ApproveMemberAsync(string slug, string targetUserId, string adminUserId);

    /// <summary>
    /// Rejects a pending join request. Caller must be a GroupAdmin.
    /// </summary>
    Task<(bool Success, string? Error)> RejectMemberAsync(string slug, string targetUserId, string adminUserId);

    /// <summary>
    /// Removes an active member. Caller must be a GroupAdmin.
    /// Last-admin guard: cannot remove the last GroupAdmin (T2, T9).
    /// </summary>
    Task<(bool Success, string? Error)> RemoveMemberAsync(string slug, string targetUserId, string adminUserId);

    /// <summary>
    /// Changes a member's role. Caller must be a GroupAdmin.
    /// Last-admin guard: cannot demote the last GroupAdmin (T2, T9).
    /// </summary>
    Task<(bool Success, string? Error)> ChangeRoleAsync(string slug, string targetUserId, string adminUserId, GroupRole newRole);

    /// <summary>
    /// Suspends or unsuspends a member. Caller must be a GroupAdmin.
    /// Last-admin guard: cannot suspend the last GroupAdmin (T2, T9).
    /// </summary>
    Task<(bool Success, string? Error)> ChangeMemberStatusAsync(string slug, string targetUserId, string adminUserId, MemberStatus newStatus);
}
