using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class ResendConfirmationCommandHandler(IAuthService authService) : IRequestHandler<ResendConfirmationCommand, CommandResult>
{
    public Task<CommandResult> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken) =>
        authService.ResendConfirmationAsync(request.Email, request.ConfirmationCallbackTemplate, request.IpAddress, request.UserAgent, cancellationToken);
}
