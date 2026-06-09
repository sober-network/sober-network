using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.News;

public record GetCommentsQuery(Guid PostId, Guid RequestingUserId) : IRequest<DataResult<IEnumerable<CommentResponse>>>;
