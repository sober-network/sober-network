namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for setting or replacing a member phone number.</summary>
public record SetPhoneRequest(
    /// <summary>Phone number to store. Required and maximum 20 characters. E.164 format is recommended.</summary>
    string PhoneNumber
);
