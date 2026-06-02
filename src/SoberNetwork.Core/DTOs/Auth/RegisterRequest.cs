namespace SoberNetwork.Core.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string? FirstName
);
