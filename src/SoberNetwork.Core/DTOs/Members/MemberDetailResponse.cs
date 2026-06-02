namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Group-scoped view of another member — only what they have opted to share (T3, T12).
/// No email, no last name. Sobriety and phone subject to their visibility settings.
/// </summary>
public record MemberDetailResponse(
    string UserId,
    string DisplayName,
    string Role,
    string Status,
    bool IsProbationary,
    SobrietyResponse? Sobriety,
    string? PhoneNumber,        // null unless IsPhoneShared=true for this group
    DateTime JoinedAt
);
