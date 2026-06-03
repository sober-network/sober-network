namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for updating the caller's phone-sharing preference within a group.</summary>
public record PhoneVisibilityRequest(
    /// <summary>Whether the caller wants to share their phone number with members of the group.</summary>
    bool IsShared
);
