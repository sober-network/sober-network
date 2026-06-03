using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SoberNetwork.Api.Tests.Infrastructure;

/// <summary>Creates signed JWTs for integration tests.</summary>
public static class JwtTestHelper
{
    /// <summary>Generates a bearer token with the claims expected by the API.</summary>
    public static string GenerateToken(
        string userId,
        bool isSuperAdmin,
        string secret,
        string issuer,
        string audience)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("isSuperAdmin", isSuperAdmin ? "true" : "false"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
