using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>
/// Handles the public cross-group meeting search. No authentication required.
/// Only returns meetings from active, publicly-listed groups (T4 — group autonomy).
/// </summary>
public class SearchPublicMeetingsQueryHandler(IMeetingService meetingService)
    : IRequestHandler<SearchPublicMeetingsQuery, IReadOnlyList<PublicMeetingSearchResponse>>
{
    public Task<IReadOnlyList<PublicMeetingSearchResponse>> Handle(
        SearchPublicMeetingsQuery request, CancellationToken cancellationToken) =>
        meetingService.SearchPublicMeetingsAsync(
            request.Days,
            request.TimeBlock,
            request.Formats,
            request.MeetingType,
            request.IsOpen,
            request.Latitude,
            request.Longitude,
            request.RadiusMiles,
            cancellationToken);
}
