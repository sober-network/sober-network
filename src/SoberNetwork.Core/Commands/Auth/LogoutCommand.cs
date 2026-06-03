using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Revokes the refresh token and ends the session.</summary>
public record LogoutCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent) : IRequest<CommandResult>;
