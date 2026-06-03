namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Request body for resending an email confirmation link.</summary>
public record ResendConfirmationRequest(
    /// <summary>Registered email address. Required and must be a valid email address.</summary>
    string Email
);
