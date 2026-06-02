namespace SoberNetwork.Core.DTOs.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string UserId,
    string Email,
    string DisplayName,
    bool IsSuperAdmin
);
