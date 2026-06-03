using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class CreateMeetingCommandHandler(IMeetingService meetingService)
    : IRequestHandler<CreateMeetingCommand, DataResult<AdminMeetingResponse>>
{
    public async Task<DataResult<AdminMeetingResponse>> Handle(
        CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var (meeting, error) = await meetingService.CreateMeetingAsync(
            request.Slug, request.Request, request.UserId, cancellationToken);
        if (error is not null)
            return DataResult<AdminMeetingResponse>.Fail(
                error.Contains("permission") || error.Contains("only admin") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<AdminMeetingResponse>.Ok(meeting!);
    }
}
