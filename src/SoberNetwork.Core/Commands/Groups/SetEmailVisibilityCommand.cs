using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

/// <summary>Updates the caller's per-group email sharing preference.</summary>
public record SetEmailVisibilityCommand(Guid UserId, string Slug, bool IsShared) : IRequest<CommandResult>;
