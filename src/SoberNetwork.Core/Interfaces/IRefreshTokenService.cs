using SoberNetwork.Core.Entities;

namespace SoberNetwork.Core.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>Creates and persists a new refresh token for the given user. Returns the plaintext token.</summary>
    Task<string> CreateAsync(string userId);

    /// <summary>
    /// Validates the plaintext token, rotates it (revokes old, issues new), and returns the new plaintext token.
    /// Returns null if the token is invalid or expired.
    /// </summary>
    Task<(string newPlainToken, RefreshToken storedToken)?> RotateAsync(string plainToken);

    /// <summary>Revokes a single refresh token by its plaintext value. No-op if not found or already revoked.</summary>
    Task RevokeAsync(string plainToken);

    /// <summary>Revokes all active refresh tokens for a user (e.g. on password reset).</summary>
    Task RevokeAllForUserAsync(string userId);
}
