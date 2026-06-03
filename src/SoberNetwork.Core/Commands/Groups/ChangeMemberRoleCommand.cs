using MediatR;
using SoberNetwork.Core.Results;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Commands.Groups;

public record ChangeMemberRoleCommand(string Slug, string TargetUserId, string AdminUserId, GroupRole NewRole) : IRequest<CommandResult>;
