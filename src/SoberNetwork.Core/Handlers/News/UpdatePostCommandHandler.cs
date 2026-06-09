using MediatR;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class UpdatePostCommandHandler(INewsService newsService) : IRequestHandler<UpdatePostCommand, DataResult<PostResponse>>
{
    public async Task<DataResult<PostResponse>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var (post, error) = await newsService.UpdatePostAsync(request.UserId, request.PostId, request.Request, cancellationToken);
        if (error is not null || post is null)
        {
            var code = error!.Contains("author") || error.Contains("permission") ? ResultCode.Forbidden
                : error.Contains("not found") ? ResultCode.NotFound
                : ResultCode.BadRequest;
            return DataResult<PostResponse>.Fail(code, error);
        }
        return DataResult<PostResponse>.Ok(post);
    }
}
