using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>Returns service-role assignments for a group the caller belongs to.</summary>
public record GetGroupServiceRolesQuery(string Slug, Guid UserId) : IRequest<DataResult<IReadOnlyList<GroupServiceRoleResponse>>>;
