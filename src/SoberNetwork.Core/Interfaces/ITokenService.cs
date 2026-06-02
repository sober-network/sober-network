using SoberNetwork.Core.Entities;

namespace SoberNetwork.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
    DateTime GetExpiry();

    /// <summary>Generates a cryptographically random refresh token. Returns (plaintext, SHA-256 hash).</summary>
    (string token, string hash) GenerateRefreshToken();
    string HashToken(string token);
    DateTime GetRefreshExpiry();
}
