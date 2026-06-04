using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

/// <summary>Soft-deletes a meeting. Caller must be a GroupAdmin.</summary>
public record DeleteMeetingCommand(string Slug, Guid MeetingId, Guid UserId) : IRequest<CommandResult>;
