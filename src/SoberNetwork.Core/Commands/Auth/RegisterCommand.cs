using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Registers a new user account.</summary>
public record RegisterCommand(
    string Email,
    string Password,
    string DisplayName,
    string? FirstName,
    string ConfirmationCallbackTemplate,
    string? IpAddress,
    string? UserAgent) : IRequest<CommandResult>;
