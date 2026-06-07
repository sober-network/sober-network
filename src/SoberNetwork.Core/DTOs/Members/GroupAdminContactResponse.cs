namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Group-scoped contact details for an active group admin.</summary>
public record GroupAdminContactResponse(
    /// <summary>Unique identifier of the member.</summary>
    Guid UserId,
    /// <summary>Display name shown to group members.</summary>
    string DisplayName,
    /// <summary>Email address for contacting the admin.</summary>
    string Email,
    /// <summary>Phone number when the admin has opted to share it with the group; otherwise null.</summary>
    string? PhoneNumber
);
