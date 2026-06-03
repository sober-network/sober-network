using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record ChangeEmailCommand(string UserId, ChangeEmailRequest Request) : IRequest<CommandResult>;
