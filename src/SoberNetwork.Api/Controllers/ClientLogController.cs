using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace SoberNetwork.Api.Controllers;

/// <summary>
/// Receives structured log entries from the Angular frontend.
/// Requires authentication — unauthenticated clients have no business writing to the application log.
/// Payload is size-limited; Data accepts only structured metadata (no free-form strings containing PII).
/// Tradition 12: logs must never contain email, display name, phone number, or sobriety date.
/// </summary>
[Authorize]
[ApiController]
[Route("api/client-log")]
public class ClientLogController(ILogger<ClientLogController> logger) : ControllerBase
{
    [HttpPost]
    public IActionResult Log([FromBody] ClientLogRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var level = (request.Level ?? "info").ToLowerInvariant() switch
        {
            "error" => LogEventLevel.Error,
            "warn"  => LogEventLevel.Warning,
            _       => LogEventLevel.Information,
        };

        // Log userId from the JWT claim — never the message's raw Data which may contain PII.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        if (level == LogEventLevel.Error)
            logger.LogError("[ANGULAR] userId={UserId} {Message} {@Data}", userId, request.Message, request.Data);
        else if (level == LogEventLevel.Warning)
            logger.LogWarning("[ANGULAR] userId={UserId} {Message} {@Data}", userId, request.Message, request.Data);
        else
            logger.LogInformation("[ANGULAR] userId={UserId} {Message} {@Data}", userId, request.Message, request.Data);

        return Ok();
    }
}

public record ClientLogRequest(
    string? Level,
    [Required, MaxLength(500)] string Message,
    // Structured metadata only — callers must not include email, name, phone, or sobriety date here (T12).
    Dictionary<string, string>? Data);
