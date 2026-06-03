using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, DataResult<AuthResponse>>
{
    public Task<DataResult<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        authService.LoginAsync(request.Email, request.Password, request.IpAddress, request.UserAgent, cancellationToken);
}
