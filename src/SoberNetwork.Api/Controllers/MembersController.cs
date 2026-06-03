using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Queries.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Member profile API — all endpoints require authentication (global [Authorize] policy).
/// No endpoint exposes PII beyond what the authenticated user has explicitly opted to share (T3, T12).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MembersController(IMediator mediator) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsSuperAdmin => User.FindFirstValue("isSuperAdmin") == "true";

    /// <summary>Returns the authenticated user's full profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken = default)
    {
        var profile = await mediator.Send(new GetMyProfileQuery(UserId), cancellationToken);
        return profile == null ? NotFound() : Ok(profile);
    }

    /// <summary>Returns the authenticated user's sobriety data.</summary>
    [HttpGet("me/sobriety")]
    public async Task<IActionResult> GetMySobriety(CancellationToken cancellationToken = default)
    {
        var profile = await mediator.Send(new GetMyProfileQuery(UserId), cancellationToken);
        return profile == null ? NotFound() : Ok(profile.Sobriety);
    }

    /// <summary>Updates display name, first name, and/or timezone.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] Core.DTOs.Members.UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateProfileCommand(UserId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>
    /// Permanently soft-deletes account. Requires password confirmation.
    /// Cascades to all group memberships and refresh tokens.
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount([FromBody] Core.DTOs.Members.DeleteAccountRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteAccountCommand(UserId, request.Password), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            ResultCode.Unauthorized => Problem(result.Error, statusCode: 401),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Changes password. Requires current password.</summary>
    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] Core.DTOs.Members.ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ChangePasswordCommand(UserId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Password updated." }),
            ResultCode.BadRequest => Problem(result.Error, statusCode: 400),
            _ => Problem(result.Error, statusCode: 401)
        };
    }

    /// <summary>
    /// Initiates email change. Requires current password.
    /// Sends a confirmation link to the new address — account is not changed until confirmed.
    /// </summary>
    [HttpPatch("me/email")]
    public async Task<IActionResult> ChangeEmail([FromBody] Core.DTOs.Members.ChangeEmailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ChangeEmailCommand(UserId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Confirmation sent to your new address. Check your email." }),
            ResultCode.Unauthorized => Problem(result.Error, statusCode: 401),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Sets or updates the sobriety date. Always private until explicitly shared (T3).</summary>
    [HttpPut("me/sobriety-date")]
    public async Task<IActionResult> SetSobrietyDate([FromBody] Core.DTOs.Members.SetSobrietyDateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new SetSobrietyDateCommand(UserId, request.SobrietyDate), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Sobriety date saved." }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Removes the sobriety date and resets all visibility flags.</summary>
    [HttpDelete("me/sobriety-date")]
    public async Task<IActionResult> RemoveSobrietyDate(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RemoveSobrietyDateCommand(UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            _ => NotFound()
        };
    }

    /// <summary>
    /// Updates sobriety visibility independently for the date and the days-sober count (T3).
    /// </summary>
    [HttpPatch("me/sobriety-date/visibility")]
    public async Task<IActionResult> UpdateSobrietyVisibility([FromBody] Core.DTOs.Members.SobrietyVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateSobrietyVisibilityCommand(UserId, request.IsDatePublic, request.IsDaysPublic), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Sobriety visibility updated." }),
            _ => NotFound()
        };
    }

    /// <summary>Sets or replaces the phone number. Sharing is off by default (T12).</summary>
    [HttpPut("me/phone")]
    public async Task<IActionResult> SetPhone([FromBody] Core.DTOs.Members.SetPhoneRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new SetPhoneCommand(UserId, request.PhoneNumber), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Phone number saved." }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Removes phone number and clears all per-group sharing flags.</summary>
    [HttpDelete("me/phone")]
    public async Task<IActionResult> RemovePhone(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RemovePhoneCommand(UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => NoContent(),
            _ => NotFound()
        };
    }

    /// <summary>Returns all users on the platform. SuperAdmin only.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllMembers(CancellationToken cancellationToken = default)
    {
        if (!IsSuperAdmin) return Forbid();
        return Ok(await mediator.Send(new GetAllMembersQuery(), cancellationToken));
    }

    /// <summary>Returns the full admin view of any user. SuperAdmin only.</summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId, CancellationToken cancellationToken = default)
    {
        if (!IsSuperAdmin) return Forbid();
        var user = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>Force-deactivates any user account. SuperAdmin only.</summary>
    [HttpPatch("{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string userId, CancellationToken cancellationToken = default)
    {
        if (!IsSuperAdmin) return Forbid();
        var result = await mediator.Send(new DeactivateUserCommand(UserId, userId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Account deactivated." }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }
}
