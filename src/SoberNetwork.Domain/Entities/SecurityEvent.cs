using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Domain.Entities;

public class SecurityEvent
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public SecurityEventType EventType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>Optional structured detail — never contains passwords, tokens, or PII beyond UserId.</summary>
    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
