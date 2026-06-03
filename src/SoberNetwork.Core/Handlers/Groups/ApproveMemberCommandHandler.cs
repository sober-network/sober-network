using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class ApproveMemberCommandHandler(IGroupService groupService) : IRequestHandler<ApproveMemberCommand, CommandResult>
{
    public async Task<CommandResult> Handle(ApproveMemberCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await groupService.ApproveMemberAsync(request.Slug, request.TargetUserId, request.AdminUserId);
        if (!success)
            return CommandResult.Fail(error!.Contains("permission") ? ResultCode.Forbidden : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}
