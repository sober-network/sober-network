using MediatR;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Authenticates a user and returns an auth token pair.</summary>
public record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent) : IRequest<DataResult<AuthResponse>>;
