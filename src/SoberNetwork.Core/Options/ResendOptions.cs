namespace SoberNetwork.Core.Options;

/// <summary>Resend email service configuration. Bound from the "Resend" configuration section.</summary>
public record ResendOptions
{
    /// <summary>The verified sender address for all outgoing emails.</summary>
    public required string FromAddress { get; init; }
}
