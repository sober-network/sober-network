namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Minimal information shown to group admins for a pending join request.</summary>
public record JoinRequestResponse(
    /// <summary>Unique identifier of the requesting member.</summary>
    string UserId,
    /// <summary>Display name of the requesting member.</summary>
    string DisplayName,
    /// <summary>UTC timestamp when the join request was submitted.</summary>
    DateTime RequestedAt
);
