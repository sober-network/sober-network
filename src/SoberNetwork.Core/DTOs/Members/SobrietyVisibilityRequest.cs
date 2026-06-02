namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Controls two independent visibility toggles (T3 — granular opt-in).
/// A member may share their days-sober count without revealing their actual date.
/// </summary>
public record SobrietyVisibilityRequest(
    bool IsDatePublic,
    bool IsDaysPublic
);
