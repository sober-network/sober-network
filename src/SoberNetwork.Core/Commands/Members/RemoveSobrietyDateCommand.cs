using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record RemoveSobrietyDateCommand(Guid UserId) : IRequest<CommandResult>;
