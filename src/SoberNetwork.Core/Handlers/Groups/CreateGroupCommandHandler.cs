using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class CreateGroupCommandHandler(IGroupService groupService) : IRequestHandler<CreateGroupCommand, DataResult<GroupResponse>>
{
    public async Task<DataResult<GroupResponse>> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var (group, error) = await groupService.CreateGroupAsync(request.Request, request.UserId, cancellationToken);
        if (error is not null)
            return DataResult<GroupResponse>.Fail(ResultCode.Conflict, error);
        return DataResult<GroupResponse>.Ok(group!);
    }
}
