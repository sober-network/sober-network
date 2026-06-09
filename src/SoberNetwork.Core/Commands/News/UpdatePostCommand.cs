using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

public record UpdatePostCommand(Guid UserId, Guid PostId, UpdatePostRequest Request) : IRequest<DataResult<PostResponse>>;
