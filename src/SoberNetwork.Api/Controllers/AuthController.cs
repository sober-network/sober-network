using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Enums;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IEmailService emailService,
    IAuditService auditService,
    AppDbContext db,
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
            logger.LogWarning("Registration failed for {Email}: {Errors}",
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
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

        // Always return the same response — no user enumeration
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
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            logger.LogWarning("Login attempt for unknown email");
            await auditService.LogAsync(SecurityEventType.LoginFailed, details: "Unknown email", ipAddress: Ip(), userAgent: Ua());
            return Unauthorized("Invalid credentials.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            await auditService.LogAsync(SecurityEventType.Lockout, user.Id, ipAddress: Ip(), userAgent: Ua());
            logger.LogWarning("Locked out login attempt: {UserId}", user.Id);
            return Unauthorized("Invalid credentials.");
        }

        if (!result.Succeeded)
        {
            await auditService.LogAsync(SecurityEventType.LoginFailed, user.Id, ipAddress: Ip(), userAgent: Ua());
            logger.LogWarning("Failed login: {UserId}", user.Id);
            return Unauthorized("Invalid credentials.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await auditService.LogAsync(SecurityEventType.LoginSuccess, user.Id, ipAddress: Ip(), userAgent: Ua());

        logger.LogInformation("Successful login: {UserId}", user.Id);
        return Ok(await BuildResponseAsync(user));
    }

    // ─── Password Reset ───────────────────────────────────────────────────────

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Always return the same response — no user enumeration
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

        // Revoke all existing refresh tokens on password change — forces re-login everywhere
        await RevokeAllRefreshTokensAsync(user.Id, "password reset");
        await auditService.LogAsync(SecurityEventType.PasswordReset, user.Id, ipAddress: Ip(), userAgent: Ua());

        logger.LogInformation("Password reset completed: {UserId}", user.Id);
        return Ok("Password reset successful. You can now log in.");
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var hash = tokenService.HashToken(request.RefreshToken);

        var stored = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored is null || !stored.IsActive)
        {
            logger.LogWarning("Invalid or expired refresh token attempt");
            return Unauthorized("Invalid or expired refresh token.");
        }

        // Rotate: revoke the old token, issue a new one
        stored.RevokedAt = DateTime.UtcNow;
        var (newToken, newHash) = tokenService.GenerateRefreshToken();
        stored.ReplacedByTokenHash = newHash;

        var newRefreshToken = new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newHash,
            ExpiresAt = tokenService.GetRefreshExpiry()
        };

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync();

        await auditService.LogAsync(SecurityEventType.TokenRefreshed, stored.UserId, ipAddress: Ip(), userAgent: Ua());

        var response = new AuthResponse(
            Token: tokenService.GenerateToken(stored.User),
            ExpiresAt: tokenService.GetExpiry(),
            RefreshToken: newToken,
            RefreshTokenExpiresAt: newRefreshToken.ExpiresAt,
            UserId: stored.User.Id,
            Email: stored.User.Email!,
            DisplayName: stored.User.DisplayName,
            IsSuperAdmin: stored.User.IsSuperAdmin
        );

        return Ok(response);
    }

    // ─── Logout ───────────────────────────────────────────────────────────────

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var hash = tokenService.HashToken(request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored is { IsActive: true })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            await auditService.LogAsync(SecurityEventType.Logout, stored.UserId, ipAddress: Ip(), userAgent: Ua());
        }

        // Always return 200 — no enumeration on logout
        return Ok("Logged out successfully.");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user)
    {
        var (refreshToken, refreshHash) = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = tokenService.GetRefreshExpiry()
        });
        await db.SaveChangesAsync();

        return new AuthResponse(
            Token: tokenService.GenerateToken(user),
            ExpiresAt: tokenService.GetExpiry(),
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),
            UserId: user.Id,
            Email: user.Email!,
            DisplayName: user.DisplayName,
            IsSuperAdmin: user.IsSuperAdmin
        );
    }

    private async Task RevokeAllRefreshTokensAsync(string userId, string reason)
    {
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
            t.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Revoked {Count} refresh token(s) for {UserId} — reason: {Reason}",
            tokens.Count, userId, reason);
    }

    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Ua() => HttpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
