using MediatR;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Notifications;

namespace SoberNetwork.Core.Handlers.Notifications;

public class GetUnreadCountQueryHandler(INotificationService notificationService)
    : IRequestHandler<GetUnreadCountQuery, int>
{
    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        => await notificationService.GetUnreadCountAsync(request.UserId, cancellationToken);
}
