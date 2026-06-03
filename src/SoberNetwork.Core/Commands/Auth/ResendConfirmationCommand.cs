using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Resends the email confirmation link.</summary>
public record ResendConfirmationCommand(
    string Email,
    string ConfirmationCallbackTemplate,
    string? IpAddress,
    string? UserAgent) : IRequest<CommandResult>;
