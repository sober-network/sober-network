using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class ForgotPasswordCommandHandler(IAuthService authService) : IRequestHandler<ForgotPasswordCommand, CommandResult>
{
    public Task<CommandResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken) =>
        authService.ForgotPasswordAsync(request.Email, request.ResetCallbackTemplate, request.IpAddress, request.UserAgent, cancellationToken);
}
