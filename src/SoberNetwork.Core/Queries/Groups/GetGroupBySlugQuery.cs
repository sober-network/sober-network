using MediatR;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Queries.Groups;

public record GetGroupBySlugQuery(string Slug, string UserId) : IRequest<GroupResponse?>;
