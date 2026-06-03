using MediatR;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Rotates a refresh token and returns a new auth token pair.</summary>
public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent) : IRequest<DataResult<AuthResponse>>;
