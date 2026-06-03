namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Authentication result returned after successful sign-in or token refresh.</summary>
public record AuthResponse(
    /// <summary>JWT access token for authenticated API requests.</summary>
    string AccessToken,
    /// <summary>UTC timestamp when the access token expires.</summary>
    DateTime ExpiresAt,
    /// <summary>Refresh token used to obtain a new access token.</summary>
    string RefreshToken,
    /// <summary>UTC timestamp when the refresh token expires.</summary>
    DateTime RefreshTokenExpiresAt,
    /// <summary>Unique identifier of the authenticated user.</summary>
    string UserId,
    /// <summary>Email address of the authenticated user.</summary>
    string Email,
    /// <summary>Display name shown in the application.</summary>
    string DisplayName,
    /// <summary>Whether the authenticated user has SuperAdmin privileges.</summary>
    bool IsSuperAdmin
);
