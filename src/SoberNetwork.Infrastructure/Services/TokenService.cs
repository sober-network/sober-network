using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Security.Cryptography;

using System.Text;

using Microsoft.Extensions.Options;

using Microsoft.IdentityModel.Tokens;

using SoberNetwork.Domain.Entities;

using SoberNetwork.Core.Interfaces;

using SoberNetwork.Core.Options;



namespace SoberNetwork.Infrastructure.Services;



public class TokenService(IOptions<JwtOptions> options) : ITokenService

{

    private readonly JwtOptions _jwt = options.Value;



    public string GenerateToken(ApplicationUser user)

    {

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



        var claims = new List<Claim>

        {

            new(ClaimTypes.NameIdentifier, user.Id),

            new(ClaimTypes.Email, user.Email!),

            new("displayName", user.DisplayName),

            new("isSuperAdmin", user.IsSuperAdmin.ToString().ToLower())

        };



        var token = new JwtSecurityToken(

            issuer: _jwt.Issuer,

            audience: _jwt.Audience,

            claims: claims,

            expires: GetExpiry(),

            signingCredentials: credentials

        );



        return new JwtSecurityTokenHandler().WriteToken(token);

    }



    public DateTime GetExpiry() => DateTime.UtcNow.AddHours(_jwt.ExpiryHours);



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



    public DateTime GetRefreshExpiry() => DateTime.UtcNow.AddDays(_jwt.RefreshExpiryDays);

}

