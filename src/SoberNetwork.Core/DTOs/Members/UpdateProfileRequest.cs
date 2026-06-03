namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for updating editable profile fields.</summary>
public record UpdateProfileRequest(
    /// <summary>Optional display name shown to other members. Maximum 100 characters.</summary>
    string? DisplayName,
    /// <summary>Optional first name. Maximum 50 characters.</summary>
    string? FirstName,
    /// <summary>Optional IANA or platform time zone identifier. Maximum 100 characters.</summary>
    string? TimeZone
);
