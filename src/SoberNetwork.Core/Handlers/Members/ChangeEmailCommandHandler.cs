using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class ChangeEmailCommandHandler(IMemberService memberService) : IRequestHandler<ChangeEmailCommand, CommandResult>
{
    public async Task<CommandResult> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.ChangeEmailAsync(request.UserId, request.Request);
        if (!success)
            return CommandResult.Fail(error!.Contains("Incorrect") ? ResultCode.Unauthorized : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
