namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Sobriety information returned by the API.</summary>
public record SobrietyResponse(
    /// <summary>Sobriety date when set and visible in the current context; otherwise null.</summary>
    DateOnly? SobrietyDate,
    /// <summary>Calculated days sober when set and visible in the current context; otherwise null.</summary>
    int? DaysSober,
    /// <summary>Whether the member has opted to share their sobriety date.</summary>
    bool IsDatePublic,
    /// <summary>Whether the member has opted to share their days-sober count.</summary>
    bool IsDaysPublic
);
