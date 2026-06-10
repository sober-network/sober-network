using MediatR;

namespace SoberNetwork.Core.Commands.Notifications;

public record MarkOneNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest;
