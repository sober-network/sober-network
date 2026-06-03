using SoberNetwork.Domain.Entities;



namespace SoberNetwork.Core.Interfaces;



/// <summary>Generates and validates JWT-related token values for authenticated sessions.</summary>

public interface ITokenService

{

    /// <summary>Generates a signed JWT access token for the specified user.</summary>

    string GenerateToken(ApplicationUser user);



    /// <summary>Returns the UTC expiration timestamp for a newly issued access token.</summary>

    DateTime GetExpiry();



    /// <summary>Generates a cryptographically random refresh token and its SHA-256 hash.</summary>

    (string token, string hash) GenerateRefreshToken();



    /// <summary>Hashes a plaintext refresh token with SHA-256 for storage.</summary>

    string HashToken(string token);



    /// <summary>Returns the UTC expiration timestamp for a newly issued refresh token.</summary>

    DateTime GetRefreshExpiry();

}

