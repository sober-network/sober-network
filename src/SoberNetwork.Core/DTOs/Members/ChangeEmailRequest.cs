namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for starting an email address change.</summary>
public record ChangeEmailRequest(
    /// <summary>Current password used to confirm the change request. Required.</summary>
    string CurrentPassword,
    /// <summary>Replacement email address. Required, valid email, maximum 256 characters.</summary>
    string NewEmail
);
