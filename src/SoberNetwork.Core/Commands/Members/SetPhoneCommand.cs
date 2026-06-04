using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record SetPhoneCommand(Guid UserId, string PhoneNumber) : IRequest<CommandResult>;
