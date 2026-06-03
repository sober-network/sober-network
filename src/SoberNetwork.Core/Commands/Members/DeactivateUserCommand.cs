using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record DeactivateUserCommand(string AdminUserId, string TargetUserId) : IRequest<CommandResult>;
