using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class ChangePasswordCommandHandler(IMemberService memberService) : IRequestHandler<ChangePasswordCommand, CommandResult>
{
    public async Task<CommandResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.ChangePasswordAsync(request.UserId, request.Request);
        if (!success)
            return CommandResult.Fail(error!.Contains("match") ? ResultCode.BadRequest : ResultCode.Unauthorized, error!);
        return CommandResult.Ok();
    }
}
