using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record DeleteAccountCommand(string UserId, string Password) : IRequest<CommandResult>;
