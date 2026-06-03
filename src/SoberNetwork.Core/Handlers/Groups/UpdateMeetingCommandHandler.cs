using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class UpdateMeetingCommandHandler(IMeetingService meetingService)
    : IRequestHandler<UpdateMeetingCommand, DataResult<AdminMeetingResponse>>
{
    public async Task<DataResult<AdminMeetingResponse>> Handle(
        UpdateMeetingCommand request, CancellationToken cancellationToken)
    {
        var (meeting, error) = await meetingService.UpdateMeetingAsync(
            request.Slug, request.MeetingId, request.Request, request.UserId, cancellationToken);
        if (error is not null)
            return DataResult<AdminMeetingResponse>.Fail(
                error.Contains("permission") || error.Contains("only admin") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<AdminMeetingResponse>.Ok(meeting!);
    }
}
