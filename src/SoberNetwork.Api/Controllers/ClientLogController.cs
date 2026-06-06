using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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
/// As defence-in-depth the server scrubs email/phone patterns from the message and metadata before logging.
/// </summary>
[Authorize]
[ApiController]
[Route("api/client-log")]
public partial class ClientLogController(ILogger<ClientLogController> logger) : ControllerBase
{
    [GeneratedRegex(@"[\w.+-]+@[\w-]+\.[\w.-]+", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\+?\d[\d().\-\s]{7,}\d")]
    private static partial Regex PhoneRegex();

    /// <summary>Redacts email and phone patterns so client-supplied text can never leak PII into logs (T12).</summary>
    private static string Scrub(string? value) =>
        value is null
            ? string.Empty
            : PhoneRegex().Replace(EmailRegex().Replace(value, "[redacted-email]"), "[redacted-phone]");

    [HttpPost]
    public IActionResult Log([FromBody] ClientLogRequest request)
    {
        var level = (request.Level ?? "info").ToLowerInvariant() switch
        {
            "error" => LogEventLevel.Error,
            "warn"  => LogEventLevel.Warning,
            _       => LogEventLevel.Information,
        };

        // Log userId from the JWT claim — never the message's raw Data which may contain PII.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var message = Scrub(request.Message);
        var data = request.Data?.ToDictionary(kv => kv.Key, kv => Scrub(kv.Value));

        if (level == LogEventLevel.Error)
            logger.LogError("[ANGULAR] userId={UserId} {Message} {@Data}", userId, message, data);
        else if (level == LogEventLevel.Warning)
            logger.LogWarning("[ANGULAR] userId={UserId} {Message} {@Data}", userId, message, data);
        else
            logger.LogInformation("[ANGULAR] userId={UserId} {Message} {@Data}", userId, message, data);

        return Ok();
    }
}

public record ClientLogRequest(
    string? Level,
    [Required, MaxLength(500)] string Message,
    // Structured metadata only — callers must not include email, name, phone, or sobriety date here (T12).
    Dictionary<string, string>? Data);
