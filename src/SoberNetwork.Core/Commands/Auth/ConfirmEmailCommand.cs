using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Confirms a user's email address.</summary>
public record ConfirmEmailCommand(
    string UserId,
    string Token,
    string? IpAddress,
    string? UserAgent) : IRequest<CommandResult>;
