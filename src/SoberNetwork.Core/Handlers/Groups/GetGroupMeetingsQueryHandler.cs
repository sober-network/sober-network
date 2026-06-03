using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetGroupMeetingsQueryHandler(IMeetingService meetingService)
    : IRequestHandler<GetGroupMeetingsQuery, DataResult<IReadOnlyList<MeetingResponse>>>
{
    public async Task<DataResult<IReadOnlyList<MeetingResponse>>> Handle(
        GetGroupMeetingsQuery request, CancellationToken cancellationToken)
    {
        var (meetings, error) = await meetingService.GetGroupMeetingsAsync(request.Slug, request.UserId, cancellationToken);
        if (error is not null)
            return DataResult<IReadOnlyList<MeetingResponse>>.Fail(
                error.Contains("permission") || error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<IReadOnlyList<MeetingResponse>>.Ok(meetings!);
    }
}
