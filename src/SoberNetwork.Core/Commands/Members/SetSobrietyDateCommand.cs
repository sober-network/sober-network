using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Members;

public record SetSobrietyDateCommand(string UserId, DateOnly SobrietyDate) : IRequest<CommandResult>;
