namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Mailing address DTO — used for chip mailing and as default "near me" search origin.
/// T3/T12 reviewed: opt-in only, never exposed publicly, accessible only to the owner.
/// </summary>
public record MailingAddressResponse(
    string? MailingStreet,
    string? MailingCity,
    string? MailingState,
    string? MailingPostalCode,
    string? MailingCountry,
    double? MailingLatitude,
    double? MailingLongitude
);

/// <summary>Request to update the authenticated user's mailing address.</summary>
public record UpdateMailingAddressRequest(
    string? MailingStreet,
    string? MailingCity,
    string? MailingState,
    string? MailingPostalCode,
    string? MailingCountry
);
