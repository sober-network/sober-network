using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoberNetwork.Core.Commands.Auth;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Api.Controllers;

// [AllowAnonymous] — auth endpoints are intentionally public; the global [Authorize] fallback
// policy is overridden here since users cannot be authenticated before registering or logging in.
// T11/T12 review: no member data, no identifiers, no PII exposed on any of these endpoints.
// Public access is required by design — you cannot authenticate before you have an account.
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Core.DTOs.Auth.RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var callbackTemplate = Url.Action(nameof(ConfirmEmail), "Auth",
            new { userId = "{userId}", token = "{token}" }, Request.Scheme)!;

        var result = await mediator.Send(new RegisterCommand(
            request.Email, request.Password, request.DisplayName, request.FirstName,
            callbackTemplate, Ip(), Ua()), cancellationToken);

        return result.Code switch
        {
            ResultCode.Ok => Ok("Registration successful. Please check your email to confirm your account."),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    // GET is required here (not POST) because confirmation links are clicked in email clients,
    // which always issue GET requests. This is a documented exception to the GET-never-mutates rule.
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ConfirmEmailCommand(userId, token, Ip(), Ua()), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok("Email confirmed. You can now log in."),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] Core.DTOs.Auth.ResendConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var callbackTemplate = Url.Action(nameof(ConfirmEmail), "Auth",
            new { userId = "{userId}", token = "{token}" }, Request.Scheme)!;

        await mediator.Send(new ResendConfirmationCommand(request.Email, callbackTemplate, Ip(), Ua()), cancellationToken);
        return Ok("If that email is registered and unconfirmed, a new confirmation link has been sent.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Core.DTOs.Auth.LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password, Ip(), Ua()), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Unauthorized => Problem(result.Error, statusCode: 401),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] Core.DTOs.Auth.ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var callbackTemplate = Url.Action(nameof(ResetPassword), "Auth",
            new { userId = "{userId}", token = "{token}" }, Request.Scheme)!;

        await mediator.Send(new ForgotPasswordCommand(request.Email, callbackTemplate, Ip(), Ua()), cancellationToken);
        return Ok("If that email is registered, a password reset link has been sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] Core.DTOs.Auth.ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ResetPasswordCommand(
            request.UserId, request.Token, request.NewPassword, Ip(), Ua()), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok("Password reset successful. You can now log in."),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] Core.DTOs.Auth.RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken, Ip(), Ua()), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Unauthorized => Problem(result.Error, statusCode: 401),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] Core.DTOs.Auth.RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new LogoutCommand(request.RefreshToken, Ip(), Ua()), cancellationToken);
        return Ok("Logged out successfully.");
    }

    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Ua() => HttpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
