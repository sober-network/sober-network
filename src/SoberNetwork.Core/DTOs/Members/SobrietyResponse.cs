namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Sobriety information returned by the API.
/// Null fields mean the data exists but the user has chosen not to share it publicly.
/// When viewing own profile, all fields are always populated.
/// </summary>
public record SobrietyResponse(
    DateOnly? SobrietyDate,      // null if not set, or not public (when viewed by others)
    int? DaysSober,              // null if not set, or not public (when viewed by others)
    bool IsDatePublic,
    bool IsDaysPublic
);
