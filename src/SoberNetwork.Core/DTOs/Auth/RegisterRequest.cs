namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Request body for creating a new member account.</summary>
public record RegisterRequest(
    /// <summary>Member email address. Required, valid email, maximum 256 characters.</summary>
    string Email,
    /// <summary>Initial account password. Required, 10 to 256 characters.</summary>
    string Password,
    /// <summary>Display name shown to other members. Required, maximum 100 characters.</summary>
    string DisplayName,
    /// <summary>Optional first name for the member profile. Maximum 100 characters.</summary>
    string? FirstName
);
