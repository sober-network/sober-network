using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetAdminMeetingsQueryHandler(IMeetingService meetingService)
    : IRequestHandler<GetAdminMeetingsQuery, DataResult<IReadOnlyList<AdminMeetingResponse>>>
{
    public async Task<DataResult<IReadOnlyList<AdminMeetingResponse>>> Handle(
        GetAdminMeetingsQuery request, CancellationToken cancellationToken)
    {
        var (meetings, error) = await meetingService.GetAdminMeetingsAsync(request.Slug, request.UserId, cancellationToken);
        if (error is not null)
            return DataResult<IReadOnlyList<AdminMeetingResponse>>.Fail(
                error.Contains("permission") || error.Contains("only admin") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<IReadOnlyList<AdminMeetingResponse>>.Ok(meetings!);
    }
}
