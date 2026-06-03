namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for updating sobriety visibility settings independently.</summary>
public record SobrietyVisibilityRequest(
    /// <summary>Whether the member allows their sobriety date to be shown where permitted.</summary>
    bool IsDatePublic,
    /// <summary>Whether the member allows their days-sober count to be shown where permitted.</summary>
    bool IsDaysPublic
);
