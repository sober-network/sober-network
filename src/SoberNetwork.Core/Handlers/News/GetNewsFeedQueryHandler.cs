using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class GetNewsFeedQueryHandler(INewsService newsService) : IRequestHandler<GetNewsFeedQuery, DataResult<PagedResponse<PostResponse>>>
{
    public async Task<DataResult<PagedResponse<PostResponse>>> Handle(GetNewsFeedQuery request, CancellationToken cancellationToken)
    {
        var (posts, error) = await newsService.GetNewsFeedAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        if (error is not null)
            return DataResult<PagedResponse<PostResponse>>.Fail(ResultCode.BadRequest, error);
        return DataResult<PagedResponse<PostResponse>>.Ok(posts!);
    }
}
