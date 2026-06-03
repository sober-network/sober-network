using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record SetPhoneCommand(string UserId, string PhoneNumber) : IRequest<CommandResult>;
