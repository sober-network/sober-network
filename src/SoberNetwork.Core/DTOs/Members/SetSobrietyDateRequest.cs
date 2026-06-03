namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for setting or updating a sobriety date.</summary>
public record SetSobrietyDateRequest(
    /// <summary>Sobriety date selected by the member. Required.</summary>
    DateOnly SobrietyDate
);
