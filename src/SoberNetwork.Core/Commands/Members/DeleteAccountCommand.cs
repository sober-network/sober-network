using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record DeleteAccountCommand(Guid UserId, string Password) : IRequest<CommandResult>;
