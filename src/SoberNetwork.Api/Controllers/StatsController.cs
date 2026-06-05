using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.Queries;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Public platform statistics — member, group, and meeting counts for the landing page.
/// T11/T12 reviewed: aggregate integer counts only, no member data exposed.
/// Equivalent to statistics published by AA Intergroup.
/// </summary>
[ApiController]
[Route("api/stats")]
[AllowAnonymous] // T11/T12 reviewed — aggregate counts only, no identifiable data. See class summary.
public class StatsController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns current counts of active members, groups, and meetings.</summary>
    [HttpGet]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetPlatformStatsQuery(), cancellationToken));
}
