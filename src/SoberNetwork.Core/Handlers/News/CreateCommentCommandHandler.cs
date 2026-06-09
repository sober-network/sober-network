using MediatR;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class CreateCommentCommandHandler(INewsService newsService) : IRequestHandler<CreateCommentCommand, DataResult<CommentResponse>>
{
    public async Task<DataResult<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var (comment, error) = await newsService.CreateCommentAsync(request.PostId, request.AuthorId, request.Request, cancellationToken);
        if (error is not null || comment is null)
        {
            var code = error!.Contains("not a member") || error.Contains("permission") ? ResultCode.Forbidden
                : error.Contains("not found") ? ResultCode.NotFound
                : ResultCode.BadRequest;
            return DataResult<CommentResponse>.Fail(code, error);
        }
        return DataResult<CommentResponse>.Ok(comment);
    }
}
