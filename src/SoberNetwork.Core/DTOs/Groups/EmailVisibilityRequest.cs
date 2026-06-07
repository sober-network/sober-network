namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request to toggle per-group email sharing.</summary>
public record EmailVisibilityRequest(
    /// <summary>Whether to share the caller's email address with members of this group.</summary>
    bool IsShared
);
