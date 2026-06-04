using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

/// <summary>Creates a new meeting within a group. Caller must be a GroupAdmin.</summary>
public record CreateMeetingCommand(string Slug, CreateMeetingRequest Request, Guid UserId)
    : IRequest<DataResult<AdminMeetingResponse>>;
