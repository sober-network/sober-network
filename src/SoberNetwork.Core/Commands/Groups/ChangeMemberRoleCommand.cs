using MediatR;
using SoberNetwork.Core.Results;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Commands.Groups;

public record ChangeMemberRoleCommand(string Slug, Guid TargetUserId, Guid AdminUserId, GroupRole NewRole) : IRequest<CommandResult>;
