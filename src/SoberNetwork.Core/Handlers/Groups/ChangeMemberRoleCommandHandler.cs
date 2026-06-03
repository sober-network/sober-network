using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class ChangeMemberRoleCommandHandler(IGroupService groupService) : IRequestHandler<ChangeMemberRoleCommand, CommandResult>
{
    public async Task<CommandResult> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await groupService.ChangeRoleAsync(request.Slug, request.TargetUserId, request.AdminUserId, request.NewRole);
        if (!success)
            return CommandResult.Fail(error!.Contains("permission") ? ResultCode.Forbidden : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
