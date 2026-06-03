using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class ConfirmEmailCommandHandler(IAuthService authService) : IRequestHandler<ConfirmEmailCommand, CommandResult>
{
    public Task<CommandResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken) =>
        authService.ConfirmEmailAsync(request.UserId, request.Token, request.IpAddress, request.UserAgent, cancellationToken);
}
