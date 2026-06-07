using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class RequestToJoinCommandHandler(IGroupService groupService) : IRequestHandler<RequestToJoinCommand, DataResult<bool>>
{
    public async Task<DataResult<bool>> Handle(RequestToJoinCommand request, CancellationToken cancellationToken)
    {
        var (success, autoApproved, error) = await groupService.RequestToJoinAsync(request.Slug, request.UserId, cancellationToken);
        if (!success)
            return DataResult<bool>.Fail(error!.Contains("already") ? ResultCode.Conflict : ResultCode.NotFound, error!);
        return DataResult<bool>.Ok(autoApproved);
    }
}
