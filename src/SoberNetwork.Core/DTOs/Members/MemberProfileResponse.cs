namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Full self-view of the authenticated user's profile.
/// All fields visible — this is only ever returned to the user themselves.
/// </summary>
public record MemberProfileResponse(
    string UserId,
    string DisplayName,
    string? FirstName,
    string Email,
    string? PhoneNumber,
    string? TimeZone,
    SobrietyResponse? Sobriety,   // null if sobriety date not set
    bool IsSuperAdmin,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
