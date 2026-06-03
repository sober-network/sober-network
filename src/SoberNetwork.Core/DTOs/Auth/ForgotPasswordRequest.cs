namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Request body for starting a password reset flow.</summary>
public record ForgotPasswordRequest(
    /// <summary>Registered email address. Required and must be a valid email address.</summary>
    string Email
);
