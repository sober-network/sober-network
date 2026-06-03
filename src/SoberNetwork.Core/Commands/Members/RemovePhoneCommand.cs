using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record RemovePhoneCommand(string UserId) : IRequest<CommandResult>;
