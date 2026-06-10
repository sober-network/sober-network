using MediatR;
using SoberNetwork.Core.Commands.Notifications;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Core.Handlers.Notifications;

public class MarkOneNotificationReadCommandHandler(INotificationService notificationService)
    : IRequestHandler<MarkOneNotificationReadCommand>
{
    public async Task Handle(MarkOneNotificationReadCommand request, CancellationToken cancellationToken)
        => await notificationService.MarkOneReadAsync(request.NotificationId, request.UserId, cancellationToken);
}
