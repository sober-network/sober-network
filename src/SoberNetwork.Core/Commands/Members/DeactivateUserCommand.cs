using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record DeactivateUserCommand(Guid AdminUserId, Guid TargetUserId) : IRequest<CommandResult>;
