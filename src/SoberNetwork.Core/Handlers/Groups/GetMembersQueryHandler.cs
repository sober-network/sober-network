using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetMembersQueryHandler(IGroupService groupService) : IRequestHandler<GetMembersQuery, DataResult<PagedResponse<MemberResponse>>>
{
    public async Task<DataResult<PagedResponse<MemberResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var (members, error) = await groupService.GetMembersAsync(request.Slug, request.UserId, request.Page, request.PageSize);
        if (error is not null)
            return DataResult<PagedResponse<MemberResponse>>.Fail(error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<PagedResponse<MemberResponse>>.Ok(members!);
    }
}
