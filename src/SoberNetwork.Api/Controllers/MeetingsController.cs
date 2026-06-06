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
/// Meeting management endpoints. Scoped under /api/groups/{slug}/meetings.
/// All endpoints require authentication — meetings data is never public (T11/T12).
/// Zoom credentials are returned only to authenticated members; admin Notes only to GroupAdmin.
/// </summary>
[Authorize]
[ApiController]
[Route("api/groups/{slug}/meetings")]
public class MeetingsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Returns all active meetings for the group.
    /// Caller must be an active member. Includes Zoom credentials; excludes admin Notes.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMeetings(string slug, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetGroupMeetingsQuery(slug, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(),
            _ => Problem(result.Error)
        };
    }

    /// <summary>
    /// Returns all meetings including admin-only Notes field.
    /// Caller must be a GroupAdmin.
    /// </summary>
    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminMeetings(string slug, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminMeetingsQuery(slug, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(),
            _ => Problem(result.Error)
        };
    }

    /// <summary>Creates a new meeting for the group. Caller must be a GroupAdmin.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateMeeting(
        string slug, [FromBody] CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreateMeetingCommand(slug, request, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(),
            _ => Problem(result.Error)
        };
    }

    /// <summary>Updates a meeting. Caller must be a GroupAdmin.</summary>
    [HttpPut("{meetingId:guid}")]
    public async Task<IActionResult> UpdateMeeting(
        string slug, Guid meetingId, [FromBody] UpdateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateMeetingCommand(slug, meetingId, request, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(),
            _ => Problem(result.Error)
        };
    }

    /// <summary>Soft-deletes a meeting. Caller must be a GroupAdmin.</summary>
    [HttpDelete("{meetingId:guid}")]
    public async Task<IActionResult> DeleteMeeting(
        string slug, Guid meetingId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteMeetingCommand(slug, meetingId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(),
            _ => Problem(result.Error)
        };
    }
}
