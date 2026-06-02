namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Member info visible to other group members — no email, phone, or sobriety date (T3, T12).
/// Display name only; last name is never included.
/// </summary>
public record MemberResponse(
    string UserId,
    string DisplayName,
    string Role,
    string Status,
    bool IsProbationary,
    DateTime JoinedAt,
    DateTime? ApprovedAt
);
