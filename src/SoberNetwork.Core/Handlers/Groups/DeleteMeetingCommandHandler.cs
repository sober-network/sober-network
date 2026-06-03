using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

public class DeleteMeetingCommandHandler(IMeetingService meetingService)
    : IRequestHandler<DeleteMeetingCommand, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await meetingService.DeleteMeetingAsync(
            request.Slug, request.MeetingId, request.UserId, cancellationToken);
        if (!success)
            return CommandResult.Fail(
                error!.Contains("permission") || error.Contains("only admin") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return CommandResult.Ok();
    }
}
