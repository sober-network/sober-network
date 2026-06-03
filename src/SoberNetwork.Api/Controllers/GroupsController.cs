using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Groups API — all endpoints require authentication (global [Authorize] policy).
/// Slugs are used in URLs, never numeric IDs (T12 — no enumerable identifiers).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroupsController(IGroupService groupService, IMemberService memberService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsSuperAdmin => User.FindFirstValue("isSuperAdmin") == "true";

    /// <summary>Returns all groups the current user is an active member of.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyGroups(CancellationToken cancellationToken = default)
    {
        var groups = await groupService.GetUserGroupsAsync(UserId);
        return Ok(groups);
    }

    /// <summary>
    /// Returns all groups on the platform. SuperAdmin only.
    /// Authorization is enforced by the controller guard (IsSuperAdmin check) rather than a service-level
    /// role assertion. Callers of IGroupService.GetAllGroupsAsync are responsible for enforcing this.
    /// [AllowAnonymous] not applicable — this is superadmin-gated, not public.
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllGroups(CancellationToken cancellationToken = default)
    {
        if (!IsSuperAdmin) return Forbid();
        var groups = await groupService.GetAllGroupsAsync();
        return Ok(groups);
    }

    /// <summary>Creates a new group. Caller becomes the initial GroupAdmin.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var (group, error) = await groupService.CreateGroupAsync(request, UserId);
        if (error != null) return Problem(error, statusCode: 409);
        return CreatedAtAction(nameof(GetGroup), new { slug = group!.Slug }, group);
    }

    /// <summary>Returns full group detail. Caller must be an active member.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetGroup(string slug, CancellationToken cancellationToken = default)
    {
        var group = await groupService.GetGroupBySlugAsync(slug, UserId);
        if (group == null) return NotFound();
        return Ok(group);
    }

    /// <summary>
    /// Public group info for newcomers and discovery/join page. No Zoom credentials, no member data.
    /// [AllowAnonymous] — intentionally public: T5 requires meeting info to be accessible to anyone
    /// seeking recovery. IsPublic controls directory listing; direct-link access is always allowed (T4 autonomy).
    /// Review: passes T3 (no PII required), T11 (no member data), T12 (no identifiers).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{slug}/info")]
    public async Task<IActionResult> GetGroupInfo(string slug, CancellationToken cancellationToken = default)
    {
        var info = await groupService.GetGroupInfoAsync(slug);
        if (info == null) return NotFound();
        return Ok(info);
    }

    /// <summary>Updates mutable group fields. Caller must be a GroupAdmin.</summary>
    [HttpPut("{slug}")]
    public async Task<IActionResult> UpdateGroup(string slug, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var (group, error) = await groupService.UpdateGroupAsync(slug, request, UserId);
        if (error != null) return error.Contains("permission") ? Forbid() : NotFound();
        return Ok(group);
    }

    /// <summary>Soft-deletes the group. Caller must be a GroupAdmin.</summary>
    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteGroup(string slug, CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.SoftDeleteGroupAsync(slug, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : NotFound();
        return NoContent();
    }

    /// <summary>Returns the active member list. Caller must be an active member.</summary>
    [HttpGet("{slug}/members")]
    public async Task<IActionResult> GetMembers(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return Problem("page must be >= 1; pageSize must be 1–100.", statusCode: 400);

        var (members, error) = await groupService.GetMembersAsync(slug, UserId, page, pageSize);
        if (error != null) return error.Contains("not a member") ? Forbid() : NotFound();
        return Ok(members);
    }

    /// <summary>Allows the current user to voluntarily leave a group.</summary>
    [HttpDelete("{slug}/members/me")]
    public async Task<IActionResult> LeaveGroup(string slug, CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.LeaveGroupAsync(slug, UserId);
        if (!success) return error!.Contains("only admin") ? Problem(error, statusCode: 409) : NotFound();
        return NoContent();
    }

    /// <summary>Submits a join request for the current user. Idempotent.</summary>
    [HttpPost("{slug}/join")]
    public async Task<IActionResult> RequestToJoin(string slug, CancellationToken cancellationToken = default)
    {
        var (success, autoApproved, error) = await groupService.RequestToJoinAsync(slug, UserId);
        if (!success) return error!.Contains("already") ? Problem(error, statusCode: 409) : NotFound();

        return autoApproved
            ? Ok(new { message = "You joined the group." })
            : Accepted(new { message = "Join request submitted. Awaiting admin approval." });
    }

    /// <summary>Returns pending join requests. Caller must be a GroupAdmin.</summary>
    [HttpGet("{slug}/join-requests")]
    public async Task<IActionResult> GetJoinRequests(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return Problem("page must be >= 1; pageSize must be 1–100.", statusCode: 400);

        var (requests, error) = await groupService.GetJoinRequestsAsync(slug, UserId, page, pageSize);
        if (error != null) return error.Contains("permission") ? Forbid() : NotFound();
        return Ok(requests);
    }

    /// <summary>Approves a pending join request. Caller must be a GroupAdmin.</summary>
    [HttpPost("{slug}/members/{userId}/approve")]
    public async Task<IActionResult> ApproveMember(string slug, string userId, CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.ApproveMemberAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : Problem(error, statusCode: 400);
        return Ok(new { message = "Member approved." });
    }

    /// <summary>Rejects a pending join request. Caller must be a GroupAdmin.</summary>
    [HttpPost("{slug}/members/{userId}/reject")]
    public async Task<IActionResult> RejectMember(string slug, string userId, CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.RejectMemberAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : Problem(error, statusCode: 400);
        return Ok(new { message = "Request rejected." });
    }

    /// <summary>Removes a member. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpDelete("{slug}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string slug, string userId, CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.RemoveMemberAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : Problem(error, statusCode: 400);
        return NoContent();
    }

    /// <summary>Changes a member's role. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpPatch("{slug}/members/{userId}/role")]
    public async Task<IActionResult> ChangeMemberRole(
        string slug,
        string userId,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.ChangeRoleAsync(slug, userId, UserId, request.NewRole);
        if (!success) return error!.Contains("permission") ? Forbid() : Problem(error, statusCode: 400);
        return Ok(new { message = "Role updated." });
    }

    /// <summary>Changes a member's status. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpPatch("{slug}/members/{userId}/status")]
    public async Task<IActionResult> ChangeMemberStatus(
        string slug,
        string userId,
        [FromBody] ChangeMemberStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.ChangeMemberStatusAsync(slug, userId, UserId, request.NewStatus);
        if (!success) return error!.Contains("permission") ? Forbid() : Problem(error, statusCode: 400);
        return Ok(new { message = "Status updated." });
    }

    /// <summary>Clears the probationary flag for a member. Caller must be a GroupAdmin.</summary>
    [HttpPatch("{slug}/members/{userId}/probation")]
    public async Task<IActionResult> ClearProbation(string slug, string userId, CancellationToken cancellationToken = default)
    {
        var (success, error) = await groupService.ClearProbationaryStatusAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : Problem(error, statusCode: 400);
        return Ok(new { message = "Probationary status cleared." });
    }

    /// <summary>
    /// Toggles per-group phone sharing for the current user.
    /// Caller must be an active member of the group (T12).
    /// </summary>
    [HttpPatch("{slug}/members/me/phone-visibility")]
    public async Task<IActionResult> SetMyPhoneVisibility(string slug, [FromBody] PhoneVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        var (success, error) = await memberService.SetGroupPhoneVisibilityAsync(UserId, slug, request.IsShared);
        if (!success) return error!.Contains("not an active member") ? Forbid() : Problem(error, statusCode: 400);
        return Ok(new { message = request.IsShared ? "Phone shared with group." : "Phone hidden from group." });
    }

    /// <summary>
    /// Returns the phone list — only members who have opted in for this group (T12).
    /// Caller must be an active member.
    /// </summary>
    [HttpGet("{slug}/phone-list")]
    public async Task<IActionResult> GetPhoneList(string slug, CancellationToken cancellationToken = default)
    {
        var (list, error) = await memberService.GetGroupPhoneListAsync(UserId, slug);
        if (error != null) return error.Contains("not a member") ? Forbid() : NotFound();
        return Ok(list);
    }

    /// <summary>
    /// Returns a group-scoped profile of another member.
    /// Respects all visibility settings — no PII leaked (T3, T12).
    /// </summary>
    [HttpGet("{slug}/members/{userId}")]
    public async Task<IActionResult> GetMemberDetail(string slug, string userId, CancellationToken cancellationToken = default)
    {
        var (member, error) = await memberService.GetMemberInGroupContextAsync(UserId, slug, userId);
        if (error != null) return error.Contains("not a member") ? Forbid() : NotFound();
        return Ok(member);
    }
}
