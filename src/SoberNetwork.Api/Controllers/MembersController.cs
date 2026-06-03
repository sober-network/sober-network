using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Member profile API — all endpoints require authentication (global [Authorize] policy).
/// No endpoint exposes PII beyond what the authenticated user has explicitly opted to share (T3, T12).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsSuperAdmin => User.FindFirstValue("isSuperAdmin") == "true";

    // ── Own profile ────────────────────────────────────────────────────────────

    /// <summary>Returns the authenticated user's full profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var profile = await memberService.GetMyProfileAsync(UserId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    /// <summary>Returns the authenticated user's sobriety data.</summary>
    [HttpGet("me/sobriety")]
    public async Task<IActionResult> GetMySobriety()
    {
        var profile = await memberService.GetMyProfileAsync(UserId);
        if (profile == null) return NotFound();
        return Ok(profile.Sobriety);
    }

    /// <summary>Updates display name, first name, and/or timezone.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (profile, error) = await memberService.UpdateProfileAsync(UserId, request);
        if (error != null) return BadRequest(new { error });
        return Ok(profile);
    }

    /// <summary>
    /// Permanently soft-deletes account. Requires password confirmation.
    /// Cascades to all group memberships and refresh tokens.
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await memberService.DeleteAccountAsync(UserId, request.Password);
        if (!success) return error!.Contains("Incorrect") ? Unauthorized(new { error }) : BadRequest(new { error });
        return NoContent();
    }

    // ── Credentials ────────────────────────────────────────────────────────────

    /// <summary>Changes password. Requires current password.</summary>
    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await memberService.ChangePasswordAsync(UserId, request);
        if (!success) return error!.Contains("match") ? BadRequest(new { error }) : Unauthorized(new { error });
        return Ok(new { message = "Password updated." });
    }

    /// <summary>
    /// Initiates email change. Requires current password.
    /// Sends a confirmation link to the new address — account is not changed until confirmed.
    /// </summary>
    [HttpPatch("me/email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await memberService.ChangeEmailAsync(UserId, request);
        if (!success) return error!.Contains("Incorrect") ? Unauthorized(new { error }) : BadRequest(new { error });
        return Ok(new { message = "Confirmation sent to your new address. Check your email." });
    }

    // ── Sobriety date ──────────────────────────────────────────────────────────

    /// <summary>Sets or updates the sobriety date. Always private until explicitly shared (T3).</summary>
    [HttpPut("me/sobriety-date")]
    public async Task<IActionResult> SetSobrietyDate([FromBody] SetSobrietyDateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await memberService.SetSobrietyDateAsync(UserId, request.SobrietyDate);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Sobriety date saved." });
    }

    /// <summary>Removes the sobriety date and resets all visibility flags.</summary>
    [HttpDelete("me/sobriety-date")]
    public async Task<IActionResult> RemoveSobrietyDate()
    {
        var success = await memberService.RemoveSobrietyDateAsync(UserId);
        if (!success) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Updates sobriety visibility independently for the date and the days-sober count (T3).
    /// </summary>
    [HttpPatch("me/sobriety-date/visibility")]
    public async Task<IActionResult> UpdateSobrietyVisibility([FromBody] SobrietyVisibilityRequest request)
    {
        var success = await memberService.UpdateSobrietyVisibilityAsync(
            UserId, request.IsDatePublic, request.IsDaysPublic);
        if (!success) return NotFound();
        return Ok(new { message = "Sobriety visibility updated." });
    }

    // ── Phone ──────────────────────────────────────────────────────────────────

    /// <summary>Sets or replaces the phone number. Sharing is off by default (T12).</summary>
    [HttpPut("me/phone")]
    public async Task<IActionResult> SetPhone([FromBody] SetPhoneRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, error) = await memberService.SetPhoneAsync(UserId, request.PhoneNumber);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Phone number saved." });
    }

    /// <summary>Removes phone number and clears all per-group sharing flags.</summary>
    [HttpDelete("me/phone")]
    public async Task<IActionResult> RemovePhone()
    {
        var success = await memberService.RemovePhoneAsync(UserId);
        if (!success) return NotFound();
        return NoContent();
    }

    // ── SuperAdmin ─────────────────────────────────────────────────────────────

    /// <summary>Returns all users on the platform. SuperAdmin only.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllMembers()
    {
        if (!IsSuperAdmin) return Forbid();
        var members = await memberService.GetAllMembersAsync();
        return Ok(members);
    }

    /// <summary>Returns the full admin view of any user. SuperAdmin only.</summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        if (!IsSuperAdmin) return Forbid();

        var user = await memberService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>Force-deactivates any user account. SuperAdmin only.</summary>
    [HttpPatch("{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        if (!IsSuperAdmin) return Forbid();

        var (success, error) = await memberService.DeactivateUserAsync(UserId, userId);
        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Account deactivated." });
    }
}
