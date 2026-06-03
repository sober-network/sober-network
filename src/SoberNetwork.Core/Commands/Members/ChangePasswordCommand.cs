using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record ChangePasswordCommand(string UserId, ChangePasswordRequest Request) : IRequest<CommandResult>;
