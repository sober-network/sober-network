using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class LogoutCommandHandler(IAuthService authService) : IRequestHandler<LogoutCommand, CommandResult>
{
    public Task<CommandResult> Handle(LogoutCommand request, CancellationToken cancellationToken) =>
        authService.LogoutAsync(request.RefreshToken, request.IpAddress, request.UserAgent, cancellationToken);
}
