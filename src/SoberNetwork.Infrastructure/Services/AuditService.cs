using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    public async Task LogAsync(
        SecurityEventType eventType,
        Guid? userId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        db.SecurityEvents.Add(new SecurityEvent
        {
            EventType = eventType,
            UserId = userId,
            Details = details,
            IpAddress = ipAddress,
            // Truncate user agent to avoid bloated log rows
            UserAgent = userAgent?[..Math.Min(userAgent.Length, 512)]
        });

        await db.SaveChangesAsync();
    }
}
