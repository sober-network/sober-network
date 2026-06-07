using System.Text;

using System.Threading.RateLimiting;

using MediatR;

using FluentValidation;

using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.HttpOverrides;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.RateLimiting;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using Resend;

using Serilog;

using Serilog.Events;

using SoberNetwork.Domain.Entities;

using SoberNetwork.Core.Interfaces;

using SoberNetwork.Core.Options;

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



builder.Services.AddDbContext<AppDbContext>(options =>

    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>

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

            ClockSkew = TimeSpan.Zero

        };

    });



var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()

    ?? ["http://localhost:4200"];



builder.Services.AddCors(options =>

    options.AddPolicy("ApiCors", policy => policy

        .WithOrigins(allowedOrigins)

        .AllowAnyHeader()

        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")

        .AllowCredentials()));



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



builder.Services.AddHsts(options =>

{

    options.MaxAge = TimeSpan.FromDays(180);

    options.IncludeSubDomains = true;

});



// MediatR — scans SoberNetwork.Core for all command/query handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<SoberNetwork.Core.Commands.Auth.LoginCommand>();
    cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(SoberNetwork.Core.Behaviors.LoggingBehavior<,>));
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IGroupServiceRoleService, GroupServiceRoleService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// Auth service
builder.Services.AddScoped<SoberNetwork.Core.Interfaces.IAuthService, SoberNetwork.Infrastructure.Services.AuthService>();



builder.Services.AddOptions();

builder.Services.AddHttpClient<ResendClient>();
builder.Services.AddHttpClient("Nominatim", c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd("SoberNetwork/1.0 (geocoding; contact=admin@sobernetwork.app)");
    c.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.Configure<ResendClientOptions>(o =>

    o.ApiToken = builder.Configuration["Resend:ApiKey"]

        ?? throw new InvalidOperationException("Resend:ApiKey is not configured."));

builder.Services.AddTransient<IResend, ResendClient>();



// Typed options — services use IOptions<T> instead of IConfiguration directly

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection("Resend"));

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));



builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<SoberNetwork.Core.Validators.Auth.LoginRequestValidator>();



builder.Services.AddAuthorizationBuilder()

    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()

        .RequireAuthenticatedUser()

        .Build());



var app = builder.Build();



app.UseForwardedHeaders(new ForwardedHeadersOptions

{

    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto

});



if (!app.Environment.IsDevelopment())

{

    app.UseHsts();

}



app.Use(async (ctx, next) =>

{

    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";

    ctx.Response.Headers["X-Frame-Options"] = "DENY";

    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";

    await next();

});



// Handle client-cancelled requests gracefully — don't return 500 when the browser aborts.
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
    {
        // Client disconnected before response was sent — this is normal (e.g. navigation away).
        // Log at Debug level only; returning 499 is advisory (client is already gone).
        Serilog.Log.Debug("Request aborted by client: {Method} {Path}", ctx.Request.Method, ctx.Request.Path.Value);
        if (!ctx.Response.HasStarted)
            ctx.Response.StatusCode = 499;
    }
});



if (app.Environment.IsDevelopment())

{

    app.UseHttpsRedirection();

}



app.UseCors("ApiCors");

app.UseRateLimiter();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();



app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))

   .AllowAnonymous();



app.UseDefaultFiles();

app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");



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
try
{
    using var scope = app.Services.CreateScope();
    var cfg         = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var seedLogger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var email       = cfg["Superuser:Email"];
    var password    = cfg["Superuser:Password"];
    var displayName = cfg["Superuser:DisplayName"] ?? "Super Admin";

    if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is null)
        {
            var superuser = new ApplicationUser
            {
                UserName       = email,
                Email          = email,
                EmailConfirmed = true,
                DisplayName    = displayName,
                IsSuperAdmin   = true,
            };
            var result = await userManager.CreateAsync(superuser, password);
            if (result.Succeeded)
                seedLogger.LogInformation("Superuser seeded (new account created).");
            else
                seedLogger.LogError("Superuser seed failed: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        else if (!existing.IsSuperAdmin)
        {
            existing.IsSuperAdmin = true;
            await userManager.UpdateAsync(existing);
            seedLogger.LogInformation("Existing user promoted to superuser.");
        }
        else
        {
            seedLogger.LogInformation("Superuser already exists — seed skipped.");
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
    seedLogger.LogError(ex, $"Superuser seed failed unexpectedly.");
}



app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }


