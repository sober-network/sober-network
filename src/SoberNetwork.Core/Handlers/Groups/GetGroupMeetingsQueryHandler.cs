using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>Handles retrieval of member-visible meetings for a group.</summary>
public class GetGroupMeetingsQueryHandler(IMeetingService meetingService)
    : IRequestHandler<GetGroupMeetingsQuery, DataResult<PagedResponse<MeetingResponse>>>
{
    /// <inheritdoc />
    public async Task<DataResult<PagedResponse<MeetingResponse>>> Handle(
        GetGroupMeetingsQuery request, CancellationToken cancellationToken)
    {
        var (meetings, error) = await meetingService.GetGroupMeetingsAsync(
            request.Slug, request.UserId, request.Page, request.PageSize,
            request.Search, request.SortBy, cancellationToken);
        if (error is not null)
            return DataResult<PagedResponse<MeetingResponse>>.Fail(
                error.Contains("permission") || error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<PagedResponse<MeetingResponse>>.Ok(meetings!);
    }
}
