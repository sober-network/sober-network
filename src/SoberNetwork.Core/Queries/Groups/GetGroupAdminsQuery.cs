using MediatR;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetGroupAdminsQuery(Guid UserId, string Slug) : IRequest<DataResult<IReadOnlyList<GroupAdminContactResponse>>>;
