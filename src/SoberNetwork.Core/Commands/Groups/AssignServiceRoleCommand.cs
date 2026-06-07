using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Commands.Groups;

/// <summary>Assigns a service role to a group member.</summary>
public record AssignServiceRoleCommand(string Slug, AssignServiceRoleRequest Request, Guid AdminUserId)
    : IRequest<DataResult<GroupServiceRoleResponse>>;
