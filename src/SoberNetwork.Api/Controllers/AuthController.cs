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
            // Log detail internally, return generic message to prevent user enumeration
            logger.LogWarning("Registration failed for {Email}: {Errors}",
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest("Unable to complete registration. Check your details and try again.");
        }

        logger.LogInformation("New user registered: {UserId}", user.Id);
        return Ok(BuildResponse(user));
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
            return Unauthorized("Invalid credentials.");   // don't reveal lockout status
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
