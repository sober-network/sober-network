using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetJoinRequestsQueryHandler(IGroupService groupService) : IRequestHandler<GetJoinRequestsQuery, DataResult<PagedResponse<JoinRequestResponse>>>
{
    public async Task<DataResult<PagedResponse<JoinRequestResponse>>> Handle(GetJoinRequestsQuery request, CancellationToken cancellationToken)
    {
        var (requests, error) = await groupService.GetJoinRequestsAsync(request.Slug, request.UserId, request.Page, request.PageSize);
        if (error is not null)
            return DataResult<PagedResponse<JoinRequestResponse>>.Fail(error.Contains("permission") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<PagedResponse<JoinRequestResponse>>.Ok(requests!);
    }
}
