using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class GetCommentsQueryHandler(INewsService newsService) : IRequestHandler<GetCommentsQuery, DataResult<IEnumerable<CommentResponse>>>
{
    public async Task<DataResult<IEnumerable<CommentResponse>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var (comments, error) = await newsService.GetCommentsAsync(request.PostId, request.RequestingUserId, cancellationToken);
        if (error is not null || comments is null)
            return DataResult<IEnumerable<CommentResponse>>.Fail(ResultCode.NotFound, error ?? "Not found.");
        return DataResult<IEnumerable<CommentResponse>>.Ok(comments);
    }
}
