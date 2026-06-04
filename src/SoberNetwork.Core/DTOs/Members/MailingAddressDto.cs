namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Mailing address DTO — used for chip mailing and as default "near me" search origin.
/// T3/T12 reviewed: opt-in only, never exposed publicly, accessible only to the owner.
/// </summary>
public record MailingAddressResponse(
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    double? Latitude,
    double? Longitude
);

/// <summary>Request to update the authenticated user's mailing address.</summary>
public record UpdateMailingAddressRequest(
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country
);
