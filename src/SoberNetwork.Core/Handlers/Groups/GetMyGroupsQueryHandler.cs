using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetMyGroupsQueryHandler(IGroupService groupService) : IRequestHandler<GetMyGroupsQuery, DataResult<PagedResponse<GroupResponse>>>
{
    public async Task<DataResult<PagedResponse<GroupResponse>>> Handle(GetMyGroupsQuery request, CancellationToken cancellationToken)
    {
        var (groups, error) = await groupService.GetUserGroupsAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        if (error is not null)
            return DataResult<PagedResponse<GroupResponse>>.Fail(ResultCode.BadRequest, error);
        return DataResult<PagedResponse<GroupResponse>>.Ok(groups!);
    }
}
