using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record SetPhoneVisibilityCommand(Guid UserId, string Slug, bool IsShared) : IRequest<CommandResult>;
