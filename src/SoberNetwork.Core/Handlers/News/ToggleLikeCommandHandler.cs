using MediatR;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class ToggleLikeCommandHandler(INewsService newsService)
    : IRequestHandler<ToggleLikeCommand, DataResult<LikeToggleResponse>>
{
    public async Task<DataResult<LikeToggleResponse>> Handle(
        ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var (likeCount, isLiked, error) = await newsService.ToggleLikeAsync(
            request.PostId, request.UserId, cancellationToken);

        if (error is not null)
        {
            var code = error.Contains("not found") ? ResultCode.NotFound : ResultCode.BadRequest;
            return DataResult<LikeToggleResponse>.Fail(code, error);
        }

        return DataResult<LikeToggleResponse>.Ok(new LikeToggleResponse(likeCount, isLiked));
    }
}
