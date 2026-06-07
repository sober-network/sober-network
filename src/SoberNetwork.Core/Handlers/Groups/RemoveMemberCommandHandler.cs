using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class RemoveMemberCommandHandler(IGroupService groupService) : IRequestHandler<RemoveMemberCommand, CommandResult>
{
    public async Task<CommandResult> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await groupService.RemoveMemberAsync(request.Slug, request.TargetUserId, request.AdminUserId, cancellationToken);
        if (!success)
            return CommandResult.Fail(error!.Contains("permission") ? ResultCode.Forbidden : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
