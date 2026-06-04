using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record ApproveMemberCommand(string Slug, Guid TargetUserId, Guid AdminUserId) : IRequest<CommandResult>;
