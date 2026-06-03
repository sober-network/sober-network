namespace SoberNetwork.Core.DTOs.Auth;

/// <summary>Request body for completing a password reset.</summary>
public record ResetPasswordRequest(
    /// <summary>Identifier of the account being reset. Required.</summary>
    string UserId,
    /// <summary>Password reset token issued by the system. Required.</summary>
    string Token,
    /// <summary>Replacement password. Required and at least 10 characters.</summary>
    string NewPassword
);
