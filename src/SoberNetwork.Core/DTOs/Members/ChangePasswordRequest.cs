namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for changing the authenticated member's password.</summary>
public record ChangePasswordRequest(
    /// <summary>Current password used to authorize the change. Required.</summary>
    string CurrentPassword,
    /// <summary>New password. Required and at least 10 characters.</summary>
    string NewPassword,
    /// <summary>Confirmation copy of the new password. Required.</summary>
    string ConfirmNewPassword
);
