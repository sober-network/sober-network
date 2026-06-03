using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>Returns all meetings for a group including admin-only Notes. Caller must be a GroupAdmin.</summary>
public record GetAdminMeetingsQuery(string Slug, string UserId) : IRequest<DataResult<IReadOnlyList<AdminMeetingResponse>>>;
