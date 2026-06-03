namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Single entry in a group phone list for members who opted in.</summary>
public record PhoneListEntryResponse(
    /// <summary>Unique identifier of the member.</summary>
    string UserId,
    /// <summary>Display name shown alongside the phone number.</summary>
    string DisplayName,
    /// <summary>Shared phone number for the member.</summary>
    string PhoneNumber
);
