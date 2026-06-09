using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

public record UpdateCommentCommand(Guid CommentId, Guid RequestingUserId, UpdateCommentRequest Request)
    : IRequest<DataResult<CommentResponse>>;
