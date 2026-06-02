using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;

namespace SoberNetwork.Api.Controllers;

[ApiController]
[Route("api/client-log")]
[AllowAnonymous]
public class ClientLogController : ControllerBase
{
    [HttpPost]
    public IActionResult Log([FromBody] ClientLogRequest request)
    {
        var level = (request.Level ?? "info").ToLowerInvariant() switch
        {
            "error" => LogEventLevel.Error,
            "warn"  => LogEventLevel.Warning,
            _       => LogEventLevel.Information,
        };

        Serilog.Log.Write(level, "[ANGULAR] {Message} {Data}", request.Message, request.Data);
        return Ok();
    }
}

public record ClientLogRequest(string? Level, string Message, object? Data);
