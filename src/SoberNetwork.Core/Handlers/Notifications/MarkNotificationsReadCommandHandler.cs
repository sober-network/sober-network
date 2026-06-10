using MediatR;
using SoberNetwork.Core.Commands.Notifications;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Notifications;

public class MarkNotificationsReadCommandHandler(INotificationService notificationService)
    : IRequestHandler<MarkNotificationsReadCommand, CommandResult>
{
    public async Task<CommandResult> Handle(
        MarkNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        await notificationService.MarkAllReadAsync(request.UserId, cancellationToken);
        return CommandResult.Ok();
    }
}
