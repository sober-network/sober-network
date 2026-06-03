using SoberNetwork.Core.DTOs;

using SoberNetwork.Core.DTOs.Groups;

using SoberNetwork.Domain.Enums;



namespace SoberNetwork.Core.Interfaces;



/// <summary>Provides group membership, discovery, and administration workflows.</summary>

public interface IGroupService

{

    /// <summary>Returns all groups the specified user is currently an active member of.</summary>

    Task<IReadOnlyList<GroupResponse>> GetUserGroupsAsync(string userId);



    /// <summary>Returns a summary listing of all active groups for SuperAdmin use.</summary>

    Task<IReadOnlyList<GroupSummaryResponse>> GetAllGroupsAsync();



    /// <summary>Returns full group detail when the caller is an active member; otherwise null.</summary>

    Task<GroupResponse?> GetGroupBySlugAsync(string slug, string userId);



    /// <summary>Returns public group information for discovery and join screens.</summary>

    Task<GroupSummaryResponse?> GetGroupInfoAsync(string slug);



    /// <summary>Creates a new group and makes the creator the initial GroupAdmin.</summary>

    Task<(GroupResponse? Group, string? Error)> CreateGroupAsync(CreateGroupRequest request, string creatorUserId);



    /// <summary>Updates mutable group fields for a GroupAdmin caller.</summary>

    Task<(GroupResponse? Group, string? Error)> UpdateGroupAsync(string slug, UpdateGroupRequest request, string userId);



    /// <summary>Soft-deletes a group when the caller is a GroupAdmin.</summary>

    Task<(bool Success, string? Error)> SoftDeleteGroupAsync(string slug, string userId);



    /// <summary>Returns a paged list of active members for a group.</summary>

    Task<(PagedResponse<MemberResponse>? Members, string? Error)> GetMembersAsync(

        string slug, string userId, int page = 1, int pageSize = 25);



    /// <summary>Returns a paged list of pending join requests for a group.</summary>

    Task<(PagedResponse<JoinRequestResponse>? Requests, string? Error)> GetJoinRequestsAsync(

        string slug, string userId, int page = 1, int pageSize = 25);



    /// <summary>Submits or reuses a join request and reports whether membership was auto-approved.</summary>

    Task<(bool Success, bool AutoApproved, string? Error)> RequestToJoinAsync(string slug, string userId);



    /// <summary>Approves a pending group join request.</summary>

    Task<(bool Success, string? Error)> ApproveMemberAsync(string slug, string targetUserId, string adminUserId);



    /// <summary>Rejects a pending group join request.</summary>

    Task<(bool Success, string? Error)> RejectMemberAsync(string slug, string targetUserId, string adminUserId);



    /// <summary>Removes a member from a group while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> RemoveMemberAsync(string slug, string targetUserId, string adminUserId);



    /// <summary>Allows a member to leave a group while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> LeaveGroupAsync(string slug, string userId);



    /// <summary>Clears the probationary flag for a group member.</summary>

    Task<(bool Success, string? Error)> ClearProbationaryStatusAsync(string slug, string targetUserId, string adminUserId);



    /// <summary>Changes a member's group role while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> ChangeRoleAsync(string slug, string targetUserId, string adminUserId, GroupRole newRole);



    /// <summary>Changes a member's status while enforcing the last-admin guard.</summary>

    Task<(bool Success, string? Error)> ChangeMemberStatusAsync(string slug, string targetUserId, string adminUserId, MemberStatus newStatus);

}

