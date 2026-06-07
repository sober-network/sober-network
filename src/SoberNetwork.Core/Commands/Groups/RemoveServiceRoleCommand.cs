using MediatR;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

/// <summary>Removes a service-role assignment from a group.</summary>
public record RemoveServiceRoleCommand(string Slug, Guid RoleId, Guid AdminUserId) : IRequest<CommandResult>;
