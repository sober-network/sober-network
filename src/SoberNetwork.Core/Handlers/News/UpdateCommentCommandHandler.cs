using MediatR;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class UpdateCommentCommandHandler(INewsService newsService) : IRequestHandler<UpdateCommentCommand, DataResult<CommentResponse>>
{
    public async Task<DataResult<CommentResponse>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var (comment, error) = await newsService.UpdateCommentAsync(request.CommentId, request.RequestingUserId, request.Request.Body, cancellationToken);
        if (error is not null || comment is null)
        {
            var code = error!.Contains("author") || error.Contains("permission") ? ResultCode.Forbidden
                : error.Contains("not found") ? ResultCode.NotFound
                : ResultCode.BadRequest;
            return DataResult<CommentResponse>.Fail(code, error);
        }
        return DataResult<CommentResponse>.Ok(comment);
    }
}
