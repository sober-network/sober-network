namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Full profile view available to SuperAdmins only.
/// Includes all fields including non-public sobriety data and phone. Never returned to end users.
/// </summary>
public record AdminMemberResponse(
    string UserId,
    string DisplayName,
    string? FirstName,
    string Email,
    string? PhoneNumber,
    string? TimeZone,
    DateOnly? SobrietyDate,
    int? DaysSober,
    bool IsSobrietyDatePublic,
    bool IsDaysSoberPublic,
    bool IsSuperAdmin,
    bool IsLockedOut,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt,
    DateTime? LastLoginAt,
    bool EmailConfirmed,
    int GroupCount
);
