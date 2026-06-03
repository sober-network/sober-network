using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using SoberNetwork.Core.DTOs.Auth;

using SoberNetwork.Domain.Entities;

using SoberNetwork.Domain.Enums;

using SoberNetwork.Core.Interfaces;



namespace SoberNetwork.Api.Controllers;



// [AllowAnonymous] — auth endpoints are intentionally public; the global [Authorize] fallback

// policy is overridden here since users cannot be authenticated before registering or logging in.

// T11/T12 review: no member data, no identifiers, no PII exposed on any of these endpoints.

// Public access is required by design — you cannot authenticate before you have an account.

[AllowAnonymous]

[ApiController]

[Route("api/[controller]")]

[EnableRateLimiting("auth")]

public class AuthController(

    UserManager<ApplicationUser> userManager,

    SignInManager<ApplicationUser> signInManager,

    ITokenService tokenService,

    IEmailService emailService,

    IAuditService auditService,

    IRefreshTokenService refreshTokenService,

    ILogger<AuthController> logger) : ControllerBase

{

    [HttpPost("register")]

    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)

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

            logger.LogWarning("Registration failed: {Errors}",

                string.Join(", ", result.Errors.Select(e => e.Description)));

            return Problem("Unable to complete registration. Check your details and try again.", statusCode: 400);

        }



        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action(

            nameof(ConfirmEmail), "Auth",

            new { userId = user.Id, token },

            Request.Scheme)!;



        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);

        await auditService.LogAsync(SecurityEventType.Register, user.Id, ipAddress: Ip(), userAgent: Ua());



        logger.LogInformation("New user registered: {UserId}", user.Id);

        return Ok("Registration successful. Please check your email to confirm your account.");

    }



    // GET is required here (not POST) because confirmation links are clicked in email clients,

    // which always issue GET requests. This is a documented exception to the GET-never-mutates rule.

    [HttpGet("confirm-email")]

    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken = default)

    {

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)

            return Problem("Invalid confirmation link.", statusCode: 400);



        var result = await userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)

        {

            logger.LogWarning("Email confirmation failed for {UserId}", userId);

            return Problem("Email confirmation failed. The link may have expired.", statusCode: 400);

        }



        await auditService.LogAsync(SecurityEventType.EmailConfirmed, userId, ipAddress: Ip(), userAgent: Ua());

        logger.LogInformation("Email confirmed for {UserId}", userId);

        return Ok("Email confirmed. You can now log in.");

    }



    [HttpPost("resend-confirmation")]

    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken = default)

    {

        var user = await userManager.FindByEmailAsync(request.Email);



        if (user is null || user.EmailConfirmed)

            return Ok("If that email is registered and unconfirmed, a new confirmation link has been sent.");



        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action(

            nameof(ConfirmEmail), "Auth",

            new { userId = user.Id, token },

            Request.Scheme)!;



        await emailService.SendEmailConfirmationAsync(user.Email!, user.DisplayName, confirmationLink);

        await auditService.LogAsync(SecurityEventType.ResendConfirmation, user.Id, ipAddress: Ip(), userAgent: Ua());



        return Ok("If that email is registered and unconfirmed, a new confirmation link has been sent.");

    }



    [HttpPost("login")]

    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)

    {

        logger.LogInformation("Login attempt from {IP}", Ip());



        var user = await userManager.FindByEmailAsync(request.Email);



        if (user is null)

        {

            logger.LogWarning("Login failed — email not found");

            await auditService.LogAsync(SecurityEventType.LoginFailed, details: "Unknown email", ipAddress: Ip(), userAgent: Ua());

            return Problem("Invalid credentials.", statusCode: 401);

        }



        if (!user.EmailConfirmed)

        {

            logger.LogWarning("Login failed — email not confirmed: {UserId}", user.Id);

            return Problem("Please confirm your email address before signing in.", statusCode: 401);

        }



        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);



        if (result.IsLockedOut)

        {

            logger.LogWarning("Login failed — account locked out: {UserId}", user.Id);

            await auditService.LogAsync(SecurityEventType.Lockout, user.Id, ipAddress: Ip(), userAgent: Ua());

            return Problem("Invalid credentials.", statusCode: 401);

        }



        if (!result.Succeeded)

        {

            logger.LogWarning("Login failed — wrong password: {UserId}", user.Id);

            await auditService.LogAsync(SecurityEventType.LoginFailed, user.Id, ipAddress: Ip(), userAgent: Ua());

            return Problem("Invalid credentials.", statusCode: 401);

        }



        user.LastLoginAt = DateTime.UtcNow;

        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        await auditService.LogAsync(SecurityEventType.LoginSuccess, user.Id, ipAddress: Ip(), userAgent: Ua());



        logger.LogInformation("Login succeeded: {UserId}", user.Id);

        return Ok(await BuildResponseAsync(user));

    }



    [HttpPost("forgot-password")]

    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)

    {

        var user = await userManager.FindByEmailAsync(request.Email);



        if (user is null || !user.EmailConfirmed)

            return Ok("If that email is registered, a password reset link has been sent.");



        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var resetLink = Url.Action(

            nameof(ResetPassword), "Auth",

            new { userId = user.Id, token },

            Request.Scheme)!;



        await emailService.SendPasswordResetAsync(user.Email!, user.DisplayName, resetLink);

        await auditService.LogAsync(SecurityEventType.ForgotPassword, user.Id, ipAddress: Ip(), userAgent: Ua());



        return Ok("If that email is registered, a password reset link has been sent.");

    }



    [HttpPost("reset-password")]

    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)

    {

        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)

            return Problem("Invalid password reset request.", statusCode: 400);



        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)

        {

            logger.LogWarning("Password reset failed for {UserId}", request.UserId);

            return Problem("Password reset failed. The link may have expired.", statusCode: 400);

        }



        await refreshTokenService.RevokeAllForUserAsync(user.Id);

        await auditService.LogAsync(SecurityEventType.PasswordReset, user.Id, ipAddress: Ip(), userAgent: Ua());



        logger.LogInformation("Password reset completed: {UserId}", user.Id);

        return Ok("Password reset successful. You can now log in.");

    }



    [HttpPost("refresh")]

    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)

    {

        var rotated = await refreshTokenService.RotateAsync(request.RefreshToken);



        if (rotated is null)

        {

            logger.LogWarning("Invalid or expired refresh token attempt");

            return Problem("Invalid or expired refresh token.", statusCode: 401);

        }



        var (newPlainToken, storedToken) = rotated.Value;

        await auditService.LogAsync(SecurityEventType.TokenRefreshed, storedToken.UserId, ipAddress: Ip(), userAgent: Ua());



        var response = new AuthResponse(

            AccessToken: tokenService.GenerateToken(storedToken.User),

            ExpiresAt: tokenService.GetExpiry(),

            RefreshToken: newPlainToken,

            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),

            UserId: storedToken.User.Id,

            Email: storedToken.User.Email!,

            DisplayName: storedToken.User.DisplayName,

            IsSuperAdmin: storedToken.User.IsSuperAdmin

        );



        return Ok(response);

    }



    [HttpPost("logout")]

    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)

    {

        await refreshTokenService.RevokeAsync(request.RefreshToken);

        await auditService.LogAsync(SecurityEventType.Logout, ipAddress: Ip(), userAgent: Ua());



        return Ok("Logged out successfully.");

    }



    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user)

    {

        var refreshToken = await refreshTokenService.CreateAsync(user.Id);



        return new AuthResponse(

            AccessToken: tokenService.GenerateToken(user),

            ExpiresAt: tokenService.GetExpiry(),

            RefreshToken: refreshToken,

            RefreshTokenExpiresAt: tokenService.GetRefreshExpiry(),

            UserId: user.Id,

            Email: user.Email!,

            DisplayName: user.DisplayName,

            IsSuperAdmin: user.IsSuperAdmin

        );

    }



    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? Ua() => HttpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;

}


