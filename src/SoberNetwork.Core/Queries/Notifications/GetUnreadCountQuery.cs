using MediatR;

namespace SoberNetwork.Core.Queries.Notifications;

public record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
