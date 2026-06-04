using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Writes audit and security event entries for important system actions.</summary>
public interface IAuditService
{
    /// <summary>Persists a security event with optional actor, detail, and request metadata.</summary>
    Task LogAsync(
        SecurityEventType eventType,
        Guid? userId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);
}
