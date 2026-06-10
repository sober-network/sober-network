using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.News;

namespace SoberNetwork.Core.Handlers.News;

public class GetNotificationPostsQueryHandler(INewsService newsService)
    : IRequestHandler<GetNotificationPostsQuery, IEnumerable<PostResponse>>
{
    public async Task<IEnumerable<PostResponse>> Handle(
        GetNotificationPostsQuery request, CancellationToken cancellationToken)
        => await newsService.GetNotificationPostsAsync(request.UserId, cancellationToken);
}
