using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>Returns meetings for a group visible to authenticated members (no admin Notes).</summary>
public record GetGroupMeetingsQuery(string Slug, Guid UserId) : IRequest<DataResult<IReadOnlyList<MeetingResponse>>>;
