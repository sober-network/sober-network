using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record UpdateSobrietyVisibilityCommand(Guid UserId, bool IsPublic) : IRequest<CommandResult>;
