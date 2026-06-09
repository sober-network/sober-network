using MediatR;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

public record CreatePostCommand(Guid UserId, CreatePostRequest Request) : IRequest<DataResult<PostResponse>>;
