using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record UpdateMailingAddressCommand(Guid UserId, UpdateMailingAddressRequest Request) : IRequest<CommandResult>;
