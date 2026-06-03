namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Full self-profile view returned to the authenticated member.</summary>
public record MemberProfileResponse(
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
    /// <summary>Sobriety information for the authenticated member, if available.</summary>
    SobrietyResponse? Sobriety,
    /// <summary>Whether the member has SuperAdmin privileges.</summary>
    bool IsSuperAdmin,
    /// <summary>UTC timestamp when the account was created.</summary>
    DateTime CreatedAt,
    /// <summary>UTC timestamp of the last successful login, if any.</summary>
    DateTime? LastLoginAt
);
