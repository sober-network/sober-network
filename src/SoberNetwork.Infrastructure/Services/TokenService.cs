using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Infrastructure.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly string _secret = config["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
    private readonly string _issuer = config["Jwt:Issuer"] ?? "sober-network";
    private readonly string _audience = config["Jwt:Audience"] ?? "sober-network";
    private readonly int _expiryHours = int.Parse(config["Jwt:ExpiryHours"] ?? "1");
    private readonly int _refreshExpiryDays = int.Parse(config["Jwt:RefreshExpiryDays"] ?? "30");

    public string GenerateToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new("displayName", user.DisplayName),
            new("isSuperAdmin", user.IsSuperAdmin.ToString().ToLower())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: GetExpiry(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiry() => DateTime.UtcNow.AddHours(_expiryHours);

    public (string token, string hash) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);
        return (token, HashToken(token));
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public DateTime GetRefreshExpiry() => DateTime.UtcNow.AddDays(_refreshExpiryDays);
}
