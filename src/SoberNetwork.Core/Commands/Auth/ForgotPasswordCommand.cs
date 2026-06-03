using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Sends a password reset link to the registered email.</summary>
public record ForgotPasswordCommand(
    string Email,
    string ResetCallbackTemplate,
    string? IpAddress,
    string? UserAgent) : IRequest<CommandResult>;
