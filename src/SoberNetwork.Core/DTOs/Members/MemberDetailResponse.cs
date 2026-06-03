namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Group-scoped view of another member that respects all visibility settings.</summary>
public record MemberDetailResponse(
    /// <summary>Unique identifier of the member.</summary>
    string UserId,
    /// <summary>Display name visible to other group members.</summary>
    string DisplayName,
    /// <summary>Member role within the group.</summary>
    string Role,
    /// <summary>Current membership status within the group.</summary>
    string Status,
    /// <summary>Whether the member is still marked as probationary.</summary>
    bool IsProbationary,
    /// <summary>Sobriety information visible in this group context, if any.</summary>
    SobrietyResponse? Sobriety,
    /// <summary>Phone number when the member has chosen to share it with this group; otherwise null.</summary>
    string? PhoneNumber,
    /// <summary>UTC timestamp when the member joined the group.</summary>
    DateTime JoinedAt
);
