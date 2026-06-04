using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record LeaveGroupCommand(string Slug, Guid UserId) : IRequest<CommandResult>;
