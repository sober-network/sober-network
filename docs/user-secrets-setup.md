# Local Development Setup — User Secrets Configuration

This document explains how to configure user secrets for local development of the Sober Network API.

## What are User Secrets?

User secrets allow you to store sensitive configuration values (API keys, connection strings, JWT secrets) **locally** without committing them to the repository. These values are stored in your user profile, not in the codebase.

## Setup Instructions

### Prerequisites

Ensure you have the .NET 10 SDK installed:

```powershell
dotnet --version
```

### Initialize User Secrets for the API Project

User secrets are scoped to a project via a `UserSecretsId` in the `.csproj` file. Check if it's already configured:

```powershell
cd src\SoberNetwork.Api
dotnet user-secrets list
```

If you see "User secrets are not configured for this project," initialize them:

```powershell
dotnet user-secrets init
```

### Set Required User Secrets

The API requires the following user secrets to run locally:

#### 1. JWT Secret

Generate a strong 32+ character secret:

```powershell
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-min-32-characters-here"
```

> **Example (not for production):**
> ```powershell
> dotnet user-secrets set "Jwt:Secret" "this-is-a-local-dev-secret-key-12345"
> ```

#### 2. Resend API Key (Email Service)

If you want to test email functionality locally, set your Resend API key:

```powershell
dotnet user-secrets set "Resend:ApiKey" "re_xxxxxxxxxxxxxxxxxxxxx"
```

> **Optional:** If you don't have a Resend account yet, skip this for now. The API will log email attempts to the console without sending.

#### 3. Superuser Credentials (First-Time Seed)

On first startup, the API seeds a superuser from user secrets. Set these to create your initial admin account:

```powershell
dotnet user-secrets set "Superuser:Email" "admin@example.com"
dotnet user-secrets set "Superuser:Password" "TempPassword123!"
```

> **⚠️ Change the password after first login!** These are temporary credentials for bootstrap only.

### Verify User Secrets

To list all configured secrets (without revealing values):

```powershell
cd src\SoberNetwork.Api
dotnet user-secrets list
```

Expected output:

```
Jwt:Secret = ****
Resend:ApiKey = ****
Superuser:Email = ****
Superuser:Password = ****
```

### Start the API

Once secrets are configured:

```powershell
cd src\SoberNetwork.Api
dotnet run
```

The API will start on `https://localhost:5001` (or check console output for the actual port).

## Troubleshooting

### "User secrets are not configured for this project"

Run:

```powershell
cd src\SoberNetwork.Api
dotnet user-secrets init
```

### "No user secrets file found"

User secrets are stored in:
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- **macOS/Linux:** `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

If the directory doesn't exist, run `dotnet user-secrets init` to create it.

### "Cannot connect to database"

Ensure PostgreSQL is running:

```powershell
docker compose up -d
```

Check connection string in `appsettings.json` or `appsettings.Development.json`.

### "JWT validation failed"

Verify the `Jwt:Secret` matches what's in `Program.cs`. Both the API and any test clients must use the same secret.

## Next Steps

1. ✅ Initialize user secrets: `dotnet user-secrets init`
2. ✅ Set `Jwt:Secret`, `Resend:ApiKey`, and superuser credentials
3. ✅ Start PostgreSQL: `docker compose up -d`
4. ✅ Run the API: `dotnet run`
5. ✅ Test with `dotnet test` (tests use `JwtTestHelper` to generate tokens)

## Further Reading

- [.NET User Secrets documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Sober Network Copilot Instructions](./.github/copilot-instructions.md)
- [Local Development PostgreSQL](./../docs/knowledge.md#local-development-setup)
