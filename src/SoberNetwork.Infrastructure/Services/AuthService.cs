using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Infrastructure.Services;

/// <summary>Authentication and session management backed by ASP.NET Core Identity.</summary>
public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IEmailService emailService,
    IAuditService auditService,
    IRefreshTokenService refreshTokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<CommandResult> RegisterAsync(string email, string password, string displayName, string? firstName, string confirmationCallbackUrl, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            FirstName = firstName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return CommandResult.Fail(ResultCode.BadRequest, "Unable to complete registration. Check your details and try again.");
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = BuildCallbackUrl(confirmationCallbackUrl, user.Id, token);

        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);
        await auditService.LogAsync(SecurityEventType.Register, user.Id, ipAddress: ipAddress, userAgent: userAgent);

        logger.LogInformation("New user registered: {UserId}", user.Id);
        return CommandResult.Ok();
    }

    public async Task<CommandResult> ConfirmEmailAsync(Guid userId, string token, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return CommandResult.Fail(ResultCode.BadRequest, "Invalid confirmation link.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            logger.LogWarning("Email confirmation failed for {UserId}", userId);
            return CommandResult.Fail(ResultCode.BadRequest, "Email confirmation failed. The link may have expired.");
        }

        await auditService.LogAsync(SecurityEventType.EmailConfirmed, userId, ipAddress: ipAddress, userAgent: userAgent);
        logger.LogInformation("Email confirmed for {UserId}", userId);
        return CommandResult.Ok();
    }

    public async Task<CommandResult> ResendConfirmationAsync(string email, string confirmationCallbackUrl, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || user.EmailConfirmed)
            return CommandResult.Ok();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = BuildCallbackUrl(confirmationCallbackUrl, user.Id, token);

        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);
        await auditService.LogAsync(SecurityEventType.ResendConfirmation, user.Id, ipAddress: ipAddress, userAgent: userAgent);
        return CommandResult.Ok();
    }

    public async Task<DataResult<AuthResponse>> LoginAsync(string email, string password, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Login attempt");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            logger.LogWarning("Login failed — email not found");
            await auditService.LogAsync(SecurityEventType.LoginFailed, details: "Unknown email", ipAddress: ipAddress, userAgent: userAgent);
            return DataResult<AuthResponse>.Fail(ResultCode.Unauthorized, "Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login failed — email not confirmed: {UserId}", user.Id);
            return DataResult<AuthResponse>.Fail(ResultCode.Unauthorized, "Please confirm your email address before signing in.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Login failed — account locked out: {UserId}", user.Id);
            await auditService.LogAsync(SecurityEventType.Lockout, user.Id, ipAddress: ipAddress, userAgent: userAgent);
            return DataResult<AuthResponse>.Fail(ResultCode.Unauthorized, "Invalid credentials.");
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed — wrong password: {UserId}", user.Id);
            await auditService.LogAsync(SecurityEventType.LoginFailed, user.Id, ipAddress: ipAddress, userAgent: userAgent);
            return DataResult<AuthResponse>.Fail(ResultCode.Unauthorized, "Invalid credentials.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await auditService.LogAsync(SecurityEventType.LoginSuccess, user.Id, ipAddress: ipAddress, userAgent: userAgent);

        logger.LogInformation("Login succeeded: {UserId}", user.Id);
        var refreshToken = await refreshTokenService.CreateAsync(user.Id, cancellationToken);
        return DataResult<AuthResponse>.Ok(new AuthResponse(
            AccessToken: tokenService.GenerateToken(user),
            ExpiresAt: tokenService.GetExpiry(),
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),
            UserId: user.Id,
            Email: user.Email!,
            DisplayName: user.DisplayName,
            IsSuperAdmin: user.IsSuperAdmin));
    }

    public async Task<CommandResult> ForgotPasswordAsync(string email, string resetCallbackUrl, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.EmailConfirmed)
            return CommandResult.Ok();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = BuildCallbackUrl(resetCallbackUrl, user.Id, token);

        await emailService.SendPasswordResetAsync(user.Email!, user.DisplayName, resetLink);
        await auditService.LogAsync(SecurityEventType.ForgotPassword, user.Id, ipAddress: ipAddress, userAgent: userAgent);
        return CommandResult.Ok();
    }

    public async Task<CommandResult> ResetPasswordAsync(Guid userId, string token, string newPassword, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return CommandResult.Fail(ResultCode.BadRequest, "Invalid password reset request.");

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Password reset failed for {UserId}", userId);
            return CommandResult.Fail(ResultCode.BadRequest, "Password reset failed. The link may have expired.");
        }

        await refreshTokenService.RevokeAllForUserAsync(user.Id, cancellationToken);
        await auditService.LogAsync(SecurityEventType.PasswordReset, user.Id, ipAddress: ipAddress, userAgent: userAgent);
        logger.LogInformation("Password reset completed: {UserId}", userId);
        return CommandResult.Ok();
    }

    public async Task<DataResult<AuthResponse>> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var rotated = await refreshTokenService.RotateAsync(refreshToken, cancellationToken);
        if (rotated is null)
        {
            logger.LogWarning("Invalid or expired refresh token attempt");
            return DataResult<AuthResponse>.Fail(ResultCode.Unauthorized, "Invalid or expired refresh token.");
        }

        var (newPlainToken, storedToken) = rotated.Value;
        await auditService.LogAsync(SecurityEventType.TokenRefreshed, storedToken.UserId, ipAddress: ipAddress, userAgent: userAgent);

        return DataResult<AuthResponse>.Ok(new AuthResponse(
            AccessToken: tokenService.GenerateToken(storedToken.User),
            ExpiresAt: tokenService.GetExpiry(),
            RefreshToken: newPlainToken,
            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),
            UserId: storedToken.User.Id,
            Email: storedToken.User.Email!,
            DisplayName: storedToken.User.DisplayName,
            IsSuperAdmin: storedToken.User.IsSuperAdmin));
    }

    public async Task<CommandResult> LogoutAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        await refreshTokenService.RevokeAsync(refreshToken, cancellationToken);
        await auditService.LogAsync(SecurityEventType.Logout, ipAddress: ipAddress, userAgent: userAgent);
        return CommandResult.Ok();
    }

    private static string BuildCallbackUrl(string callbackTemplate, Guid userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);

        return callbackTemplate
            .Replace("{userId}", userId.ToString(), StringComparison.Ordinal)
            .Replace("{token}", encodedToken, StringComparison.Ordinal)
            .Replace("%7BuserId%7D", userId.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("%7Btoken%7D", encodedToken, StringComparison.OrdinalIgnoreCase);
    }
}
