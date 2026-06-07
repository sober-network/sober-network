using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetAllGroupsQueryHandler(IGroupService groupService) : IRequestHandler<GetAllGroupsQuery, DataResult<PagedResponse<GroupSummaryResponse>>>
{
    public async Task<DataResult<PagedResponse<GroupSummaryResponse>>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        var (groups, error) = await groupService.GetAllGroupsAsync(request.Page, request.PageSize, cancellationToken);
        if (error is not null)
            return DataResult<PagedResponse<GroupSummaryResponse>>.Fail(ResultCode.BadRequest, error);
        return DataResult<PagedResponse<GroupSummaryResponse>>.Ok(groups!);
    }
}
