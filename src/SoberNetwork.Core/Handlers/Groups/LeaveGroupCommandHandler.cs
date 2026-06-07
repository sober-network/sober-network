using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class LeaveGroupCommandHandler(IGroupService groupService) : IRequestHandler<LeaveGroupCommand, CommandResult>
{
    public async Task<CommandResult> Handle(LeaveGroupCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await groupService.LeaveGroupAsync(request.Slug, request.UserId, cancellationToken);
        if (!success)
            return CommandResult.Fail(error!.Contains("only admin") ? ResultCode.Conflict : ResultCode.NotFound, error!);
        return CommandResult.Ok();
    }
}
