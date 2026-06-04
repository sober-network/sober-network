using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Public meeting finder — cross-group, no authentication required.
/// T11/T12 reviewed: meeting schedules (day, time, location, format) are non-member data,
/// equivalent to what AA Intergroup publishes publicly. No member data is exposed.
/// Only meetings from active, publicly-listed groups are returned (IsPublic=true, T4 — group autonomy).
/// Zoom credentials and admin Notes are never included.
/// </summary>
[ApiController]
[Route("api/meetings")]
[AllowAnonymous] // T11/T12 reviewed — meeting schedules are public information. See class summary.
public class PublicMeetingsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Search public meeting schedules across all publicly-listed groups.
    /// Supports filtering by day, time of day, format, meeting type, open/closed status,
    /// and optional location-based radius search.
    /// </summary>
    /// <param name="days">Comma-separated day numbers (0=Sun..6=Sat). Example: days=1,3,5</param>
    /// <param name="timeBlock">Time of day: Morning (6-12), Afternoon (12-17), Evening (17-21), Night (21-6)</param>
    /// <param name="formats">Comma-separated formats: Discussion, Speaker, StepStudy, BigBook, Beginners</param>
    /// <param name="meetingType">InPerson (0), Online (1), or Hybrid (2)</param>
    /// <param name="isOpen">true = open meetings only, false = closed only</param>
    /// <param name="lat">Origin latitude for distance search</param>
    /// <param name="lon">Origin longitude for distance search</param>
    /// <param name="radiusMiles">Search radius in miles (default 25). Used only when lat+lon provided.</param>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int[]? days,
        [FromQuery] TimeBlock? timeBlock,
        [FromQuery] string[]? formats,
        [FromQuery] MeetingType? meetingType,
        [FromQuery] bool? isOpen,
        [FromQuery] double? lat,
        [FromQuery] double? lon,
        [FromQuery] double? radiusMiles,
        CancellationToken cancellationToken = default)
    {
        var results = await mediator.Send(
            new SearchPublicMeetingsQuery(days, timeBlock, formats, meetingType, isOpen, lat, lon, radiusMiles),
            cancellationToken);

        return Ok(results);
    }
}
