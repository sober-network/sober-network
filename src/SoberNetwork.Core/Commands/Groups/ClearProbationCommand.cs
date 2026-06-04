using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record ClearProbationCommand(string Slug, Guid TargetUserId, Guid AdminUserId) : IRequest<CommandResult>;
