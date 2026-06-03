using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class ResetPasswordCommandHandler(IAuthService authService) : IRequestHandler<ResetPasswordCommand, CommandResult>
{
    public Task<CommandResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken) =>
        authService.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword, request.IpAddress, request.UserAgent, cancellationToken);
}
