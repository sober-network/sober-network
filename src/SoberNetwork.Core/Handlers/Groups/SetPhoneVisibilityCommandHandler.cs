using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class SetPhoneVisibilityCommandHandler(IMemberService memberService) : IRequestHandler<SetPhoneVisibilityCommand, CommandResult>
{
    public async Task<CommandResult> Handle(SetPhoneVisibilityCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.SetGroupPhoneVisibilityAsync(request.UserId, request.Slug, request.IsShared, cancellationToken);
        if (!success)
            return CommandResult.Fail(error!.Contains("not an active member") ? ResultCode.Forbidden : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
