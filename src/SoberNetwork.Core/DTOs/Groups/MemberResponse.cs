namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Member information visible to other group members. Contact fields only shown when opted-in (T3/T12).</summary>
public record MemberResponse(
    /// <summary>Unique identifier of the member.</summary>
    Guid UserId,
    /// <summary>Display name visible within the group.</summary>
    string DisplayName,
    /// <summary>Role of the member within the group.</summary>
    string Role,
    /// <summary>Current membership status.</summary>
    string Status,
    /// <summary>Whether the member is still marked as probationary.</summary>
    bool IsProbationary,
    /// <summary>UTC timestamp when the member joined the group.</summary>
    DateTime JoinedAt,
    /// <summary>UTC timestamp when the member was approved, if applicable.</summary>
    DateTime? ApprovedAt,
    /// <summary>Email address — only present when the member has opted in to email sharing for this group (T3/T12).</summary>
    string? Email,
    /// <summary>Phone number — only present when the member has opted in to phone sharing for this group (T3/T12).</summary>
    string? PhoneNumber,
    /// <summary>Sobriety date — only present when the member has opted in to sobriety visibility.</summary>
    DateOnly? SobrietyDate
);
