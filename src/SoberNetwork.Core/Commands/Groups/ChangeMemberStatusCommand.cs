using MediatR;
using SoberNetwork.Core.Results;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Commands.Groups;

public record ChangeMemberStatusCommand(string Slug, Guid TargetUserId, Guid AdminUserId, MemberStatus NewStatus) : IRequest<CommandResult>;
