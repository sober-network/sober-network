using MediatR;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Auth;

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, DataResult<AuthResponse>>
{
    public Task<DataResult<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
        authService.RefreshAsync(request.RefreshToken, request.IpAddress, request.UserAgent, cancellationToken);
}
