using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record RejectMemberCommand(string Slug, Guid TargetUserId, Guid AdminUserId) : IRequest<CommandResult>;
