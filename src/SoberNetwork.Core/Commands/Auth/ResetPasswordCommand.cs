using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Auth;

/// <summary>Resets the password using a reset token.</summary>
public record ResetPasswordCommand(
    string UserId,
    string Token,
    string NewPassword,
    string? IpAddress,
    string? UserAgent) : IRequest<CommandResult>;
