namespace SoberNetwork.Core.DTOs.Members;

/// <summary>Request body for updating sobriety visibility. Controls both date and days-sober count as a unit.</summary>
public record SobrietyVisibilityRequest(
    /// <summary>When true, both the sobriety date and the days-sober count are visible to group members (T3).</summary>
    bool IsPublic
);
