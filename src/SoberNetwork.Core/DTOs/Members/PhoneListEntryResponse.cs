namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// A single entry in the group phone list — user ID, first name + phone only (T11, T12).
/// No last name, no email, no sobriety info.
/// </summary>
public record PhoneListEntryResponse(
    string UserId,
    string DisplayName,
    string PhoneNumber
);
