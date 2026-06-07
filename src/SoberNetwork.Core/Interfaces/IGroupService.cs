using SoberNetwork.Core.DTOs;

using SoberNetwork.Core.DTOs.Groups;

using SoberNetwork.Domain.Enums;



namespace SoberNetwork.Core.Interfaces;



/// <summary>Provides group membership, discovery, and administration workflows.</summary>

public interface IGroupService

{

    /// <summary>Returns all groups the specified user is currently an active member of.</summary>

    Task<(PagedResponse<GroupResponse>? Groups, string? Error)> GetUserGroupsAsync(Guid userId, int page = 1, int pageSize = 25, CancellationToken ct = default);



    /// <summary>Returns a summary listing of all active groups for SuperAdmin use.</summary>

    Task<(PagedResponse<GroupSummaryResponse>? Groups, string? Error)> GetAllGroupsAsync(int page = 1, int pageSize = 25, CancellationToken ct = default);



    /// <summary>Returns full group detail when the caller is an active member; otherwise null.</summary>

    Task<GroupResponse?> GetGroupBySlugAsync(string slug, Guid userId, CancellationToken ct = default);



    /// <summary>Returns public group information for discovery and join screens.</summary>

    Task<GroupSummaryResponse?> GetGroupInfoAsync(string slug, CancellationToken ct = default);



    /// <summary>Creates a new group and makes the creator the initial GroupAdmin.</summary>

    Task<(GroupResponse? Group, string? Error)> CreateGroupAsync(CreateGroupRequest request, Guid creatorUserId, CancellationToken ct = default);



    /// <summary>Updates mutable group fields for a GroupAdmin caller.</summary>

    Task<(GroupResponse? Group, string? Error)> UpdateGroupAsync(string slug, UpdateGroupRequest request, Guid userId, CancellationToken ct = default);



    /// <summary>Soft-deletes a group when the caller is a GroupAdmin.</summary>

    Task<(bool Success, string? Error)> SoftDeleteGroupAsync(string slug, Guid userId, CancellationToken ct = default);



    /// <summary>Returns a paged list of active members for a group.</summary>

    Task<(PagedResponse<MemberResponse>? Members, string? Error)> GetMembersAsync(
        string slug, Guid userId, int page = 1, int pageSize = 25, CancellationToken ct = default);



    /// <summary>Returns a paged list of pending join requests for a group.</summary>

    Task<(PagedResponse<JoinRequestResponse>? Requests, string? Error)> GetJoinRequestsAsync(
        string slug, Guid userId, int page = 1, int pageSize = 25, CancellationToken ct = default);



    /// <summary>Submits or reuses a join request and reports whether membership was auto-approved.</summary>

    Task<(bool Success, bool AutoApproved, string? Error)> RequestToJoinAsync(string slug, Guid userId, CancellationToken ct = default);



    /// <summary>Approves a pending group join request.</summary>

    Task<(bool Success, string? Error)> ApproveMemberAsync(string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default);



    /// <summary>Rejects a pending group join request.</summary>

    Task<(bool Success, string? Error)> RejectMemberAsync(string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default);



    /// <summary>Removes a member from a group while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> RemoveMemberAsync(string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default);



    /// <summary>Allows a member to leave a group while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> LeaveGroupAsync(string slug, Guid userId, CancellationToken ct = default);



    /// <summary>Clears the probationary flag for a group member.</summary>

    Task<(bool Success, string? Error)> ClearProbationaryStatusAsync(string slug, Guid targetUserId, Guid adminUserId, CancellationToken ct = default);



    /// <summary>Changes a member's group role while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> ChangeRoleAsync(string slug, Guid targetUserId, Guid adminUserId, GroupRole newRole, CancellationToken ct = default);



    /// <summary>Changes a member's status while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> ChangeMemberStatusAsync(string slug, Guid targetUserId, Guid adminUserId, MemberStatus newStatus, CancellationToken ct = default);

}

