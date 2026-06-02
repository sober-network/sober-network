using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Enums;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Groups API — all endpoints require authentication (global [Authorize] policy).
/// Slugs are used in URLs, never numeric IDs (T12 — no enumerable identifiers).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroupsController(IGroupService groupService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsSuperAdmin => User.FindFirstValue("isSuperAdmin") == "true";

    // ── Group endpoints ────────────────────────────────────────────────────────

    /// <summary>Returns all groups the current user is an active member of.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyGroups()
    {
        var groups = await groupService.GetUserGroupsAsync(UserId);
        return Ok(groups);
    }

    /// <summary>
    /// Returns all groups on the platform. SuperAdmin only.
    /// [AllowAnonymous] not applicable — this is superadmin-gated, not public.
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllGroups()
    {
        if (!IsSuperAdmin) return Forbid();
        var groups = await groupService.GetAllGroupsAsync();
        return Ok(groups);
    }

    /// <summary>Creates a new group. Caller becomes the initial GroupAdmin.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (group, error) = await groupService.CreateGroupAsync(request, UserId);
        if (error != null) return Conflict(new { error });
        return CreatedAtAction(nameof(GetGroup), new { slug = group!.Slug }, group);
    }

    /// <summary>Returns full group detail. Caller must be an active member.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetGroup(string slug)
    {
        var group = await groupService.GetGroupBySlugAsync(slug, UserId);
        if (group == null) return NotFound();
        return Ok(group);
    }

    /// <summary>Updates mutable group fields. Caller must be a GroupAdmin.</summary>
    [HttpPut("{slug}")]
    public async Task<IActionResult> UpdateGroup(string slug, [FromBody] UpdateGroupRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (group, error) = await groupService.UpdateGroupAsync(slug, request, UserId);
        if (error != null) return error.Contains("permission") ? Forbid() : NotFound();
        return Ok(group);
    }

    /// <summary>Soft-deletes the group. Caller must be a GroupAdmin.</summary>
    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteGroup(string slug)
    {
        var (success, error) = await groupService.SoftDeleteGroupAsync(slug, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : NotFound();
        return NoContent();
    }

    // ── Member endpoints ───────────────────────────────────────────────────────

    /// <summary>Returns the active member list. Caller must be an active member.</summary>
    [HttpGet("{slug}/members")]
    public async Task<IActionResult> GetMembers(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest(new { error = "page must be >= 1; pageSize must be 1–100." });

        var (members, error) = await groupService.GetMembersAsync(slug, UserId, page, pageSize);
        if (error != null) return error.Contains("not a member") ? Forbid() : NotFound();
        return Ok(members);
    }

    /// <summary>Allows the current user to voluntarily leave a group.</summary>
    [HttpDelete("{slug}/members/me")]
    public async Task<IActionResult> LeaveGroup(string slug)
    {
        var (success, error) = await groupService.LeaveGroupAsync(slug, UserId);
        if (!success) return error!.Contains("only admin") ? Conflict(new { error }) : NotFound();
        return NoContent();
    }

    /// <summary>Submits a join request for the current user. Idempotent.</summary>
    [HttpPost("{slug}/join")]
    public async Task<IActionResult> RequestToJoin(string slug)
    {
        var (success, error) = await groupService.RequestToJoinAsync(slug, UserId);
        if (!success) return error!.Contains("already") ? Conflict(new { error }) : NotFound();
        return Accepted(new { message = "Join request submitted. Awaiting admin approval." });
    }

    /// <summary>Returns pending join requests. Caller must be a GroupAdmin.</summary>
    [HttpGet("{slug}/join-requests")]
    public async Task<IActionResult> GetJoinRequests(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest(new { error = "page must be >= 1; pageSize must be 1–100." });

        var (requests, error) = await groupService.GetJoinRequestsAsync(slug, UserId, page, pageSize);
        if (error != null) return error.Contains("permission") ? Forbid() : NotFound();
        return Ok(requests);
    }

    /// <summary>Approves a pending join request. Caller must be a GroupAdmin.</summary>
    [HttpPost("{slug}/members/{userId}/approve")]
    public async Task<IActionResult> ApproveMember(string slug, string userId)
    {
        var (success, error) = await groupService.ApproveMemberAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : BadRequest(new { error });
        return Ok(new { message = "Member approved." });
    }

    /// <summary>Rejects a pending join request. Caller must be a GroupAdmin.</summary>
    [HttpPost("{slug}/members/{userId}/reject")]
    public async Task<IActionResult> RejectMember(string slug, string userId)
    {
        var (success, error) = await groupService.RejectMemberAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : BadRequest(new { error });
        return Ok(new { message = "Request rejected." });
    }

    /// <summary>Removes a member. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpDelete("{slug}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string slug, string userId)
    {
        var (success, error) = await groupService.RemoveMemberAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : BadRequest(new { error });
        return NoContent();
    }

    /// <summary>Changes a member's role. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpPatch("{slug}/members/{userId}/role")]
    public async Task<IActionResult> ChangeMemberRole(
        string slug, string userId, [FromBody] ChangeRoleRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await groupService.ChangeRoleAsync(slug, userId, UserId, request.NewRole);
        if (!success) return error!.Contains("permission") ? Forbid() : BadRequest(new { error });
        return Ok(new { message = "Role updated." });
    }

    /// <summary>Changes a member's status. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpPatch("{slug}/members/{userId}/status")]
    public async Task<IActionResult> ChangeMemberStatus(
        string slug, string userId, [FromBody] ChangeMemberStatusRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await groupService.ChangeMemberStatusAsync(slug, userId, UserId, request.NewStatus);
        if (!success) return error!.Contains("permission") ? Forbid() : BadRequest(new { error });
        return Ok(new { message = "Status updated." });
    }

    /// <summary>Clears the probationary flag for a member. Caller must be a GroupAdmin.</summary>
    [HttpPatch("{slug}/members/{userId}/probation")]
    public async Task<IActionResult> ClearProbation(string slug, string userId)
    {
        var (success, error) = await groupService.ClearProbationaryStatusAsync(slug, userId, UserId);
        if (!success) return error!.Contains("permission") ? Forbid() : BadRequest(new { error });
        return Ok(new { message = "Probationary status cleared." });
    }
}
