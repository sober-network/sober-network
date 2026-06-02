using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Serilog;
using Serilog.Events;
using SoberNetwork.Core.Entities;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;
using SoberNetwork.Infrastructure.Services;

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with lockout and email confirmation
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 10;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero   // no grace period on expiry
        };
    });

// CORS — only allow configured origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
    options.AddPolicy("ApiCors", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
        .AllowCredentials()));

// Rate limiting — 5 requests per minute on auth endpoints
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
});

// HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(180);
    options.IncludeSubDomains = true;
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMemberService, MemberService>();

// Resend email service
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
    o.ApiToken = builder.Configuration["Resend:ApiKey"]
        ?? throw new InvalidOperationException("Resend:ApiKey is not configured."));
builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.AddControllers();

// Require authentication globally — every endpoint is protected by default.
// Use [AllowAnonymous] explicitly on public endpoints with a justification comment.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

// Handle forwarded headers from Fly.io's TLS-terminating proxy
// so X-Forwarded-Proto is respected and HttpContext.Request.IsHttps is correct
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Security headers on every response
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// Only redirect to HTTPS locally — in production Fly.io handles TLS termination at the edge
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("ApiCors");
app.UseRateLimiter();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

// Health check for Fly.io — unauthenticated by design; returns 200 to confirm liveness
// AllowAnonymous: this endpoint has no user data and must be reachable before auth is established
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .AllowAnonymous();

// Serve Angular SPA from wwwroot (populated by Dockerfile during build)
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

// SPA fallback — any route not matched by the API or static files serves index.html
// so Angular's client-side router handles navigation
app.MapFallbackToFile("index.html");

// Auto-apply pending EF Core migrations on startup.
// Wrapped in try/catch so a transient DB connectivity issue doesn't crash the process.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
catch (Exception ex)
{
    var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
    startupLogger.LogError(ex, "Database migration failed on startup — the app will start but DB may be unavailable.");
}

// Seed superuser from config (Superuser:Email / Superuser:Password / Superuser:DisplayName).
// Idempotent — skipped if the account already exists.
try
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var cfg = app.Configuration;

    var email       = cfg["Superuser:Email"];
    var password    = cfg["Superuser:Password"];
    var displayName = cfg["Superuser:DisplayName"] ?? "Super Admin";

    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is null)
        {
            var superuser = new ApplicationUser
            {
                UserName      = email,
                Email         = email,
                EmailConfirmed = true,
                DisplayName   = displayName,
                IsSuperAdmin  = true,
            };
            var result = await userManager.CreateAsync(superuser, password);
            if (result.Succeeded)
                seedLogger.LogInformation("Superuser created: {Email}", email);
            else
                seedLogger.LogError("Superuser seed failed: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        else if (!existing.IsSuperAdmin)
        {
            existing.IsSuperAdmin = true;
            await userManager.UpdateAsync(existing);
            seedLogger.LogInformation("Existing user promoted to superuser: {Email}", email);
        }
        else
        {
            seedLogger.LogInformation("Superuser already exists: {Email}", email);
        }
    }
    else
    {
        seedLogger.LogWarning("Superuser:Email or Superuser:Password not configured — skipping seed.");
    }
}
catch (Exception ex)
{
    var seedLogger = app.Services.GetRequiredService<ILogger<Program>>();
    seedLogger.LogError(ex, "Superuser seed failed unexpectedly.");
}

app.Run();
