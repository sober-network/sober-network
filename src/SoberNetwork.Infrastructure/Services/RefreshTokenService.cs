using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

public class RefreshTokenService(
    AppDbContext db,
    ITokenService tokenService,
    ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    public async Task<string> CreateAsync(string userId)
    {
        var (plainToken, hash) = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = tokenService.GetRefreshExpiry()
        });

        await db.SaveChangesAsync();
        return plainToken;
    }

    public async Task<(string newPlainToken, RefreshToken storedToken)?> RotateAsync(string plainToken)
    {
        var hash = tokenService.HashToken(plainToken);

        var stored = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored is null || !stored.IsActive)
            return null;

        var (newPlainToken, newHash) = tokenService.GenerateRefreshToken();

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = newHash;

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newHash,
            ExpiresAt = tokenService.GetRefreshExpiry()
        });

        await db.SaveChangesAsync();
        return (newPlainToken, stored);
    }

    public async Task RevokeAsync(string plainToken)
    {
        var hash = tokenService.HashToken(plainToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored is { IsActive: true })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
            t.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Revoked {Count} refresh token(s) for user {UserId}", tokens.Count, userId);
    }
}
