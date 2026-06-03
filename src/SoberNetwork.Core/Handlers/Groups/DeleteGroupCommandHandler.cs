using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class DeleteGroupCommandHandler(IGroupService groupService) : IRequestHandler<DeleteGroupCommand, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await groupService.SoftDeleteGroupAsync(request.Slug, request.UserId);
        if (!success)
            return CommandResult.Fail(error!.Contains("permission") ? ResultCode.Forbidden : ResultCode.NotFound, error!);
        return CommandResult.Ok();
    }
}
