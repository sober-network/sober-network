namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Request body for refreshing or revoking an authenticated session.</summary>
public record RefreshTokenRequest(
    /// <summary>Plaintext refresh token previously issued to the client. Required.</summary>
    string RefreshToken
);
