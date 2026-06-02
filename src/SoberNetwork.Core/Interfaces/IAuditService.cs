using SoberNetwork.Core.Enums;

namespace SoberNetwork.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        SecurityEventType eventType,
        string? userId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);
}
