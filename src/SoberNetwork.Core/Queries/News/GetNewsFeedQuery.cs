using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.News;

public record GetNewsFeedQuery(Guid UserId, int Page, int PageSize) : IRequest<DataResult<PagedResponse<PostResponse>>>;
