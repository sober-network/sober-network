using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

public record DeleteCommentCommand(Guid CommentId, Guid RequestingUserId)
    : IRequest<CommandResult>;
