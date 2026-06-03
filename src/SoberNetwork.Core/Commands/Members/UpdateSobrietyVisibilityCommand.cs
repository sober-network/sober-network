using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record UpdateSobrietyVisibilityCommand(string UserId, bool IsDatePublic, bool IsDaysPublic) : IRequest<CommandResult>;
