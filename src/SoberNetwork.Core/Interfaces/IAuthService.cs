using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Owns all authentication and session workflows.</summary>
public interface IAuthService
{
    /// <summary>Registers a new user and sends a confirmation email.</summary>
    Task<CommandResult> RegisterAsync(string email, string password, string displayName, string? firstName, string confirmationCallbackUrl, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Confirms the user's email via the token generated during registration.</summary>
    Task<CommandResult> ConfirmEmailAsync(Guid userId, string token, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Resends the email confirmation link if the account is unconfirmed.</summary>
    Task<CommandResult> ResendConfirmationAsync(string email, string confirmationCallbackUrl, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Validates credentials and returns an auth response with access and refresh tokens.</summary>
    Task<DataResult<AuthResponse>> LoginAsync(string email, string password, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Sends a password reset link if the email is registered and confirmed.</summary>
    Task<CommandResult> ForgotPasswordAsync(string email, string resetCallbackUrl, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Resets the password and revokes all refresh tokens for the user.</summary>
    Task<CommandResult> ResetPasswordAsync(Guid userId, string token, string newPassword, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Rotates a refresh token and returns a new auth response.</summary>
    Task<DataResult<AuthResponse>> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Revokes a refresh token, ending the session.</summary>
    Task<CommandResult> LogoutAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
