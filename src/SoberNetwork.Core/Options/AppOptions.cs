namespace SoberNetwork.Core.Options;

/// <summary>General application configuration. Bound from the "App" configuration section.</summary>
public record AppOptions
{
    /// <summary>Public base URL of the application used when building links in emails.</summary>
    public string BaseUrl { get; init; } = "https://app.sobernetwork.group";
}
