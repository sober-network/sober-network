namespace SoberNetwork.Core.DTOs.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string DisplayName,
    bool IsSuperAdmin
);
