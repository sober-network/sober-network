using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

/// <summary>Updates a meeting within a group. Caller must be a GroupAdmin.</summary>
public record UpdateMeetingCommand(string Slug, Guid MeetingId, UpdateMeetingRequest Request, Guid UserId)
    : IRequest<DataResult<AdminMeetingResponse>>;
