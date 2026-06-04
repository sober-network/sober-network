using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Groups API — all endpoints require authentication (global [Authorize] policy).
/// Slugs are used in URLs, never numeric IDs (T12 — no enumerable identifiers).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroupsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsSuperAdmin => User.FindFirstValue("isSuperAdmin") == "true";

    /// <summary>Returns all groups the current user is an active member of.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyGroups(CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetMyGroupsQuery(UserId), cancellationToken));

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
        return Ok(await mediator.Send(new GetAllGroupsQuery(), cancellationToken));
    }

    /// <summary>Creates a new group. Caller becomes the initial GroupAdmin.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreateGroupCommand(request, UserId), cancellationToken);
        if (!result.Success) return Problem(result.Error, statusCode: 409);
        return CreatedAtAction(nameof(GetGroup), new { slug = result.Data!.Slug }, result.Data);
    }

    /// <summary>Returns full group detail. Caller must be an active member.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetGroup(string slug, CancellationToken cancellationToken = default)
    {
        var group = await mediator.Send(new GetGroupBySlugQuery(slug, UserId), cancellationToken);
        return group == null ? NotFound() : Ok(group);
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
        var info = await mediator.Send(new GetGroupInfoQuery(slug), cancellationToken);
        return info == null ? NotFound() : Ok(info);
    }

    /// <summary>Updates mutable group fields. Caller must be a GroupAdmin.</summary>
    [HttpPut("{slug}")]
    public async Task<IActionResult> UpdateGroup(string slug, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateGroupCommand(slug, request, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            _ => NotFound()
        };
    }

    /// <summary>Soft-deletes the group. Caller must be a GroupAdmin.</summary>
    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteGroup(string slug, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteGroupCommand(slug, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            ResultCode.Forbidden => Forbid(),
            _ => NotFound()
        };
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

        var result = await mediator.Send(new GetMembersQuery(slug, UserId, page, pageSize), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            _ => NotFound()
        };
    }

    /// <summary>Allows the current user to voluntarily leave a group.</summary>
    [HttpDelete("{slug}/members/me")]
    public async Task<IActionResult> LeaveGroup(string slug, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new LeaveGroupCommand(slug, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            ResultCode.Conflict => Problem(result.Error, statusCode: 409),
            _ => NotFound()
        };
    }

    /// <summary>Submits a join request for the current user. Idempotent.</summary>
    [HttpPost("{slug}/join")]
    public async Task<IActionResult> RequestToJoin(string slug, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RequestToJoinCommand(slug, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok when result.Data => Ok(new { message = "You joined the group." }),
            ResultCode.Ok => Accepted(new { message = "Join request submitted. Awaiting admin approval." }),
            ResultCode.Conflict => Problem(result.Error, statusCode: 409),
            _ => NotFound()
        };
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

        var result = await mediator.Send(new GetJoinRequestsQuery(slug, UserId, page, pageSize), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            _ => NotFound()
        };
    }

    /// <summary>Approves a pending join request. Caller must be a GroupAdmin.</summary>
    [HttpPost("{slug}/members/{userId}/approve")]
    public async Task<IActionResult> ApproveMember(string slug, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ApproveMemberCommand(slug, userId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Member approved." }),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Rejects a pending join request. Caller must be a GroupAdmin.</summary>
    [HttpPost("{slug}/members/{userId}/reject")]
    public async Task<IActionResult> RejectMember(string slug, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RejectMemberCommand(slug, userId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Request rejected." }),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Removes a member. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpDelete("{slug}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string slug, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RemoveMemberCommand(slug, userId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Changes a member's role. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpPatch("{slug}/members/{userId}/role")]
    public async Task<IActionResult> ChangeMemberRole(
        string slug,
        Guid userId,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ChangeMemberRoleCommand(slug, userId, UserId, request.NewRole), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Role updated." }),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Changes a member's status. Caller must be a GroupAdmin. Last-admin guard enforced.</summary>
    [HttpPatch("{slug}/members/{userId}/status")]
    public async Task<IActionResult> ChangeMemberStatus(
        string slug,
        Guid userId,
        [FromBody] ChangeMemberStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ChangeMemberStatusCommand(slug, userId, UserId, request.NewStatus), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Status updated." }),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Clears the probationary flag for a member. Caller must be a GroupAdmin.</summary>
    [HttpPatch("{slug}/members/{userId}/probation")]
    public async Task<IActionResult> ClearProbation(string slug, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ClearProbationCommand(slug, userId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Probationary status cleared." }),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>
    /// Toggles per-group phone sharing for the current user.
    /// Caller must be an active member of the group (T12).
    /// </summary>
    [HttpPatch("{slug}/members/me/phone-visibility")]
    public async Task<IActionResult> SetMyPhoneVisibility(string slug, [FromBody] PhoneVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new SetPhoneVisibilityCommand(UserId, slug, request.IsShared), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = request.IsShared ? "Phone shared with group." : "Phone hidden from group." }),
            ResultCode.Forbidden => Forbid(),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>
    /// Returns the phone list — only members who have opted in for this group (T12).
    /// Caller must be an active member.
    /// </summary>
    [HttpGet("{slug}/phone-list")]
    public async Task<IActionResult> GetPhoneList(string slug, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetPhoneListQuery(UserId, slug), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            _ => NotFound()
        };
    }

    /// <summary>
    /// Returns a group-scoped profile of another member.
    /// Respects all visibility settings — no PII leaked (T3, T12).
    /// </summary>
    [HttpGet("{slug}/members/{userId}")]
    public async Task<IActionResult> GetMemberDetail(string slug, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMemberDetailQuery(UserId, slug, userId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            _ => NotFound()
        };
    }
}
