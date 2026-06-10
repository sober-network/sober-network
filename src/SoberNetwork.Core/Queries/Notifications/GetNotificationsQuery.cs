using MediatR;
using SoberNetwork.Core.DTOs.Notifications;

namespace SoberNetwork.Core.Queries.Notifications;

public record GetNotificationsQuery(Guid UserId) : IRequest<IEnumerable<NotificationResponse>>;
