using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

public record RemoveMemberCommand(string Slug, string TargetUserId, string AdminUserId) : IRequest<CommandResult>;
