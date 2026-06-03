using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class UpdateGroupCommandHandler(IGroupService groupService) : IRequestHandler<UpdateGroupCommand, DataResult<GroupResponse>>
{
    public async Task<DataResult<GroupResponse>> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var (group, error) = await groupService.UpdateGroupAsync(request.Slug, request.Request, request.UserId);
        if (error is not null)
            return DataResult<GroupResponse>.Fail(error.Contains("permission") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<GroupResponse>.Ok(group!);
    }
}
