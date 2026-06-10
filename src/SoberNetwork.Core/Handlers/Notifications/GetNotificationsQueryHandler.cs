using MediatR;
using SoberNetwork.Core.DTOs.Notifications;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Notifications;

namespace SoberNetwork.Core.Handlers.Notifications;

public class GetNotificationsQueryHandler(INotificationService notificationService)
    : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationResponse>>
{
    public async Task<IEnumerable<NotificationResponse>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
        => await notificationService.GetNotificationsAsync(request.UserId, cancellationToken);
}
