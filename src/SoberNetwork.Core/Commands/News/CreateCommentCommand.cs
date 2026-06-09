using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

public record CreateCommentCommand(Guid PostId, Guid AuthorId, CreateCommentRequest Request)
    : IRequest<DataResult<CommentResponse>>;
