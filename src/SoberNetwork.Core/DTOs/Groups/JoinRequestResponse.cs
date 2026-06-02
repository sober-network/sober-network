namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Minimal info shown to group admins for pending join requests (T3 — minimum necessary info).
/// </summary>
public record JoinRequestResponse(
    string UserId,
    string DisplayName,
    DateTime RequestedAt
);
