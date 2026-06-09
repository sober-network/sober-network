using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

public record DeletePostCommand(Guid UserId, Guid PostId) : IRequest<CommandResult>;
