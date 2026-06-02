using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IEmailService emailService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            FirstName = request.FirstName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed for {Email}: {Errors}",
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest("Unable to complete registration. Check your details and try again.");
        }

        // Send email confirmation
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(
            nameof(ConfirmEmail), "Auth",
            new { userId = user.Id, token },
            Request.Scheme)!;

        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);

        logger.LogInformation("New user registered, confirmation email sent: {UserId}", user.Id);
        return Ok("Registration successful. Please check your email to confirm your account.");
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest("Invalid confirmation link.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            logger.LogWarning("Email confirmation failed for {UserId}", userId);
            return BadRequest("Email confirmation failed. The link may have expired.");
        }

        logger.LogInformation("Email confirmed for {UserId}", userId);
        return Ok("Email confirmed. You can now log in.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            logger.LogWarning("Login attempt for unknown email: {Email}", request.Email);
            return Unauthorized("Invalid credentials.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Locked out account login attempt: {UserId}", user.Id);
            return Unauthorized("Invalid credentials.");
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Failed login for user: {UserId}", user.Id);
            return Unauthorized("Invalid credentials.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        logger.LogInformation("Successful login: {UserId}", user.Id);
        return Ok(BuildResponse(user));
    }

    private AuthResponse BuildResponse(ApplicationUser user) =>
        new(
            Token: tokenService.GenerateToken(user),
            ExpiresAt: tokenService.GetExpiry(),
            UserId: user.Id,
            Email: user.Email!,
            DisplayName: user.DisplayName,
            IsSuperAdmin: user.IsSuperAdmin
        );
}
