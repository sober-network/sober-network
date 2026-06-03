namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for permanently soft-deleting the authenticated account.</summary>
public record DeleteAccountRequest(
    /// <summary>Current password used to confirm account deletion. Required.</summary>
    string Password
);
