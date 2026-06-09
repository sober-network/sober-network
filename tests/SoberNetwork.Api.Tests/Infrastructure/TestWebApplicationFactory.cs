using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Api.Tests.Infrastructure;

/// <summary>Creates an API host configured for integration testing with mock services.</summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
    };

    public Mock<IGroupService> GroupService { get; } = new();
    public Mock<IMemberService> MemberService { get; } = new();
    public Mock<IMeetingService> MeetingService { get; } = new();
    public Mock<IAuthService> AuthService { get; } = new();
    public Mock<IEmailService> EmailService { get; } = new();
    public Mock<ITokenService> TokenService { get; } = new();
    public Mock<IAuditService> AuditService { get; } = new();
    public Mock<IRefreshTokenService> RefreshTokenService { get; } = new();

    public const string TestJwtSecret = "test-super-secret-key-for-integration-tests-must-be-long-enough";
    public const string TestJwtIssuer = "test-issuer";
    public const string TestJwtAudience = "test-audience";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestJwtSecret,
                ["Jwt:Issuer"] = TestJwtIssuer,
                ["Jwt:Audience"] = TestJwtAudience,
                ["Jwt:ExpiryMinutes"] = "60",
                ["Resend:ApiKey"] = "test-resend-key",
                ["App:BaseUrl"] = "https://localhost",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.RemoveAll<IGroupService>();
            services.RemoveAll<IMemberService>();
            services.RemoveAll<IMeetingService>();
            services.RemoveAll<IAuthService>();
            services.RemoveAll<IEmailService>();
            services.RemoveAll<ITokenService>();
            services.RemoveAll<IAuditService>();
            services.RemoveAll<IRefreshTokenService>();

            services.AddScoped<IGroupService>(_ => GroupService.Object);
            services.AddScoped<IMemberService>(_ => MemberService.Object);
            services.AddScoped<IMeetingService>(_ => MeetingService.Object);
            services.AddScoped<IAuthService>(_ => AuthService.Object);
            services.AddScoped<IEmailService>(_ => EmailService.Object);
            services.AddScoped<ITokenService>(_ => TokenService.Object);
            services.AddScoped<IAuditService>(_ => AuditService.Object);
            services.AddScoped<IRefreshTokenService>(_ => RefreshTokenService.Object);
        });
    }

    /// <summary>Creates an anonymous client.</summary>
    public HttpClient CreateAnonymousClient() => CreateClient(ClientOptions);

    /// <summary>Creates an authenticated client for a regular member.</summary>
    public HttpClient CreateAuthenticatedClient(Guid userId = default)
    {
        var token = JwtTestHelper.GenerateToken(userId, false, TestJwtSecret, TestJwtIssuer, TestJwtAudience);
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Creates an authenticated client for a SuperAdmin.</summary>
    public HttpClient CreateSuperAdminClient(Guid userId = default)
    {
        var token = JwtTestHelper.GenerateToken(userId, true, TestJwtSecret, TestJwtIssuer, TestJwtAudience);
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
