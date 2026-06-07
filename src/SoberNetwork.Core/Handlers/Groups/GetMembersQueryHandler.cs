using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>Handles retrieval of a paged, searchable, sortable member list for a group.</summary>
public class GetMembersQueryHandler(IGroupService groupService) : IRequestHandler<GetMembersQuery, DataResult<PagedResponse<MemberResponse>>>
{
    /// <inheritdoc />
    public async Task<DataResult<PagedResponse<MemberResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var (members, error) = await groupService.GetMembersAsync(
            request.Slug, request.UserId, request.Page, request.PageSize,
            request.Search, request.SortBy, request.SortDescending, cancellationToken);
        if (error is not null)
            return DataResult<PagedResponse<MemberResponse>>.Fail(error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<PagedResponse<MemberResponse>>.Ok(members!);
    }
}
