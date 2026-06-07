using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetAdminMeetingsQueryHandler(IMeetingService meetingService)
    : IRequestHandler<GetAdminMeetingsQuery, DataResult<PagedResponse<AdminMeetingResponse>>>
{
    public async Task<DataResult<PagedResponse<AdminMeetingResponse>>> Handle(
        GetAdminMeetingsQuery request, CancellationToken cancellationToken)
    {
        var (meetings, error) = await meetingService.GetAdminMeetingsAsync(request.Slug, request.UserId, request.Page, request.PageSize, cancellationToken);
        if (error is not null)
            return DataResult<PagedResponse<AdminMeetingResponse>>.Fail(
                error.Contains("permission") || error.Contains("only admin") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<PagedResponse<AdminMeetingResponse>>.Ok(meetings!);
    }
}
