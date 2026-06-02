namespace SoberNetwork.Core.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password
);
