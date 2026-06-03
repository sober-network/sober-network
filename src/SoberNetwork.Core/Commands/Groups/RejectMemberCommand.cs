using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record RejectMemberCommand(string Slug, string TargetUserId, string AdminUserId) : IRequest<CommandResult>;
