using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.News;

/// <summary>Toggles a like on a post. Returns updated like count and liked state.</summary>
public record ToggleLikeCommand(Guid PostId, Guid UserId) : IRequest<DataResult<LikeToggleResponse>>;

public record LikeToggleResponse(int LikeCount, bool IsLiked);
