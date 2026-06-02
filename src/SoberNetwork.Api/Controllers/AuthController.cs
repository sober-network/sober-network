using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Enums;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Api.Controllers;

// [AllowAnonymous] — auth endpoints are intentionally public; the global [Authorize] fallback
// policy is overridden here since users cannot be authenticated before registering or logging in.
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IEmailService emailService,
    IAuditService auditService,
    IRefreshTokenService refreshTokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    // ─── Registration ────────────────────────────────────────────────────────

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            FirstName = request.FirstName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest("Unable to complete registration. Check your details and try again.");
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(
            nameof(ConfirmEmail), "Auth",
            new { userId = user.Id, token },
            Request.Scheme)!;

        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);
        await auditService.LogAsync(SecurityEventType.Register, user.Id, ipAddress: Ip(), userAgent: Ua());

        logger.LogInformation("New user registered: {UserId}", user.Id);
        return Ok("Registration successful. Please check your email to confirm your account.");
    }

    // ─── Email Confirmation ───────────────────────────────────────────────────

    // GET is required here (not POST) because confirmation links are clicked in email clients,
    // which always issue GET requests. This is a documented exception to the GET-never-mutates rule.
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest("Invalid confirmation link.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            logger.LogWarning("Email confirmation failed for {UserId}", userId);
            return BadRequest("Email confirmation failed. The link may have expired.");
        }

        await auditService.LogAsync(SecurityEventType.EmailConfirmed, userId, ipAddress: Ip(), userAgent: Ua());
        logger.LogInformation("Email confirmed for {UserId}", userId);
        return Ok("Email confirmed. You can now log in.");
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Always return the same response — no user enumeration (T12)
        if (user is null || user.EmailConfirmed)
            return Ok("If that email is registered and unconfirmed, a new confirmation link has been sent.");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(
            nameof(ConfirmEmail), "Auth",
            new { userId = user.Id, token },
            Request.Scheme)!;

        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);
        await auditService.LogAsync(SecurityEventType.ResendConfirmation, user.Id, ipAddress: Ip(), userAgent: Ua());

        return Ok("If that email is registered and unconfirmed, a new confirmation link has been sent.");
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        logger.LogInformation("Login attempt: {Email} from {IP}", request.Email, Ip());

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            logger.LogWarning("Login failed — email not found: {Email}", request.Email);
            await auditService.LogAsync(SecurityEventType.LoginFailed, details: "Unknown email", ipAddress: Ip(), userAgent: Ua());
            return Unauthorized("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login failed — email not confirmed: {UserId}", user.Id);
            return Unauthorized("Please confirm your email address before signing in.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Login failed — account locked out: {UserId}", user.Id);
            await auditService.LogAsync(SecurityEventType.Lockout, user.Id, ipAddress: Ip(), userAgent: Ua());
            return Unauthorized("Invalid credentials.");
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed — wrong password: {UserId}", user.Id);
            await auditService.LogAsync(SecurityEventType.LoginFailed, user.Id, ipAddress: Ip(), userAgent: Ua());
            return Unauthorized("Invalid credentials.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await auditService.LogAsync(SecurityEventType.LoginSuccess, user.Id, ipAddress: Ip(), userAgent: Ua());

        logger.LogInformation("Login succeeded: {UserId}", user.Id);
        return Ok(await BuildResponseAsync(user));
    }

    // ─── Password Reset ───────────────────────────────────────────────────────

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Always return the same response — no user enumeration (T12)
        if (user is null || !user.EmailConfirmed)
            return Ok("If that email is registered, a password reset link has been sent.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action(
            nameof(ResetPassword), "Auth",
            new { userId = user.Id, token },
            Request.Scheme)!;

        await emailService.SendPasswordResetAsync(user.Email!, user.DisplayName, resetLink);
        await auditService.LogAsync(SecurityEventType.ForgotPassword, user.Id, ipAddress: Ip(), userAgent: Ua());

        return Ok("If that email is registered, a password reset link has been sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return BadRequest("Invalid password reset request.");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Password reset failed for {UserId}", request.UserId);
            return BadRequest("Password reset failed. The link may have expired.");
        }

        // Revoke all sessions on password change — forces re-login on all devices
        await refreshTokenService.RevokeAllForUserAsync(user.Id);
        await auditService.LogAsync(SecurityEventType.PasswordReset, user.Id, ipAddress: Ip(), userAgent: Ua());

        logger.LogInformation("Password reset completed: {UserId}", user.Id);
        return Ok("Password reset successful. You can now log in.");
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var rotated = await refreshTokenService.RotateAsync(request.RefreshToken);

        if (rotated is null)
        {
            logger.LogWarning("Invalid or expired refresh token attempt");
            return Unauthorized("Invalid or expired refresh token.");
        }

        var (newPlainToken, storedToken) = rotated.Value;
        await auditService.LogAsync(SecurityEventType.TokenRefreshed, storedToken.UserId, ipAddress: Ip(), userAgent: Ua());

        var response = new AuthResponse(
            AccessToken: tokenService.GenerateToken(storedToken.User),
            ExpiresAt: tokenService.GetExpiry(),
            RefreshToken: newPlainToken,
            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),
            UserId: storedToken.User.Id,
            Email: storedToken.User.Email!,
            DisplayName: storedToken.User.DisplayName,
            IsSuperAdmin: storedToken.User.IsSuperAdmin
        );

        return Ok(response);
    }

    // ─── Logout ───────────────────────────────────────────────────────────────

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken);
        await auditService.LogAsync(SecurityEventType.Logout, ipAddress: Ip(), userAgent: Ua());

        // Always return 200 — no token enumeration on logout (T12)
        return Ok("Logged out successfully.");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user)
    {
        var refreshToken = await refreshTokenService.CreateAsync(user.Id);

        return new AuthResponse(
            AccessToken: tokenService.GenerateToken(user),
            ExpiresAt: tokenService.GetExpiry(),
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),
            UserId: user.Id,
            Email: user.Email!,
            DisplayName: user.DisplayName,
            IsSuperAdmin: user.IsSuperAdmin
        );
    }

    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Ua() => HttpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
