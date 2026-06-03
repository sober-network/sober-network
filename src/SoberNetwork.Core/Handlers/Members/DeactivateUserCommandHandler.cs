using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class DeactivateUserCommandHandler(IMemberService memberService) : IRequestHandler<DeactivateUserCommand, CommandResult>
{
    public async Task<CommandResult> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.DeactivateUserAsync(request.AdminUserId, request.TargetUserId);
        if (!success)
            return CommandResult.Fail(ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
