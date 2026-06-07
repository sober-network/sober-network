using SoberNetwork.Domain.Entities;



namespace SoberNetwork.Core.Interfaces;



/// <summary>Creates, rotates, and revokes refresh tokens for authenticated sessions.</summary>

public interface IRefreshTokenService

{

    /// <summary>Creates and persists a new refresh token for the specified user.</summary>

    Task<string> CreateAsync(Guid userId, CancellationToken ct = default);



    /// <summary>Validates and rotates a refresh token, returning the new plaintext token when successful.</summary>

    Task<(string newPlainToken, RefreshToken storedToken)?> RotateAsync(string plainToken, CancellationToken ct = default);



    /// <summary>Revokes a single refresh token by its plaintext value.</summary>

    Task RevokeAsync(string plainToken, CancellationToken ct = default);



    /// <summary>Revokes all active refresh tokens for the specified user.</summary>

    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

}

