using MediatR;
using SoberNetwork.Core.DTOs.News;

namespace SoberNetwork.Core.Queries.News;

/// <summary>Returns posts that the user has unread notifications about.</summary>
public record GetNotificationPostsQuery(Guid UserId) : IRequest<IEnumerable<PostResponse>>;
