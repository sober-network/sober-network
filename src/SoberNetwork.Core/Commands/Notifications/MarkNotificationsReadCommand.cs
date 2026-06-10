using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Notifications;

public record MarkNotificationsReadCommand(Guid UserId) : IRequest<CommandResult>;
