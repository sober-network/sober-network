using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class RegisterCommandHandler(IAuthService authService) : IRequestHandler<RegisterCommand, CommandResult>
{
    public Task<CommandResult> Handle(RegisterCommand request, CancellationToken cancellationToken) =>
        authService.RegisterAsync(request.Email, request.Password, request.DisplayName, request.FirstName,
            request.ConfirmationCallbackTemplate, request.IpAddress, request.UserAgent, cancellationToken);
}
