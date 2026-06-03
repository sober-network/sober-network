namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Full member view returned to SuperAdmins only.</summary>
public record AdminMemberResponse(
    /// <summary>Unique identifier of the member.</summary>
    string UserId,
    /// <summary>Display name shown in the application.</summary>
    string DisplayName,
    /// <summary>Optional first name stored on the profile.</summary>
    string? FirstName,
    /// <summary>Member email address.</summary>
    string Email,
    /// <summary>Stored phone number, if any.</summary>
    string? PhoneNumber,
    /// <summary>Configured time zone, if any.</summary>
    string? TimeZone,
    /// <summary>Stored sobriety date, if any.</summary>
    DateOnly? SobrietyDate,
    /// <summary>Calculated number of sober days, if available.</summary>
    int? DaysSober,
    /// <summary>Whether the member has opted to share their sobriety date.</summary>
    bool IsSobrietyDatePublic,
    /// <summary>Whether the member has opted to share their days-sober count.</summary>
    bool IsDaysSoberPublic,
    /// <summary>Whether the member has SuperAdmin privileges.</summary>
    bool IsSuperAdmin,
    /// <summary>Whether the account is currently locked out.</summary>
    bool IsLockedOut,
    /// <summary>UTC timestamp when the account was created.</summary>
    DateTime CreatedAt,
    /// <summary>UTC timestamp of the most recent profile update.</summary>
    DateTime UpdatedAt,
    /// <summary>UTC timestamp when the account was soft-deleted, if applicable.</summary>
    DateTime? DeletedAt,
    /// <summary>UTC timestamp of the last successful login, if any.</summary>
    DateTime? LastLoginAt,
    /// <summary>Whether the member has confirmed their email address.</summary>
    bool EmailConfirmed,
    /// <summary>Number of active group memberships for the member.</summary>
    int GroupCount
);
