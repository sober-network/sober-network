namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Request body for member login.</summary>
public record LoginRequest(
    /// <summary>Registered email address. Required, valid email, maximum 256 characters.</summary>
    string Email,
    /// <summary>Account password. Required, maximum 256 characters.</summary>
    string Password
);
