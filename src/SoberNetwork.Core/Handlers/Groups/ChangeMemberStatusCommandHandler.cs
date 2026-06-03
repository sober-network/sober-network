using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class ChangeMemberStatusCommandHandler(IGroupService groupService) : IRequestHandler<ChangeMemberStatusCommand, CommandResult>
{
    public async Task<CommandResult> Handle(ChangeMemberStatusCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await groupService.ChangeMemberStatusAsync(request.Slug, request.TargetUserId, request.AdminUserId, request.NewStatus);
        if (!success)
            return CommandResult.Fail(error!.Contains("permission") ? ResultCode.Forbidden : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
