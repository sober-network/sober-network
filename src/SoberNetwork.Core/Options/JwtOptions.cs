namespace SoberNetwork.Core.Options;

/// <summary>JWT configuration options. Bound from the "Jwt" configuration section.</summary>
public record JwtOptions
{
    /// <summary>HMAC-SHA256 signing secret. Must be at least 32 characters.</summary>
    public required string Secret { get; init; }

    /// <summary>Token issuer claim value.</summary>
    public string Issuer { get; init; } = "sober-network";

    /// <summary>Token audience claim value.</summary>
    public string Audience { get; init; } = "sober-network";

    /// <summary>Access token lifetime in hours.</summary>
    public int ExpiryHours { get; init; } = 1;

    /// <summary>Refresh token lifetime in days.</summary>
    public int RefreshExpiryDays { get; init; } = 30;
}
