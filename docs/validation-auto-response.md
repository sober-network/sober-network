# Validation Auto-Response Pattern

## Overview

**"Validation auto-response"** is the mechanism where FluentValidation automatically intercepts incoming HTTP requests, validates them against registered validators, and returns a **400 Bad Request** with validation errors — **all without writing any code in your controller**.

This is achieved through:
1. **Auto-registration** of validators from the `SoberNetwork.Core.Validators` assembly
2. **Auto-validation middleware** that runs before your controller action is invoked
3. **Convention-based matching** — validator classes are automatically paired with their DTO types

---

## How It Works

### 1. Setup in `Program.cs`

```csharp
// Line ~210 in Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<SoberNetwork.Core.Validators.Auth.LoginRequestValidator>();
```

**What this does:**
- `AddFluentValidationAutoValidation()` — Adds ASP.NET Core middleware to automatically validate `[FromBody]` requests
- `AddValidatorsFromAssemblyContaining<LoginRequestValidator>()` — Scans `SoberNetwork.Core` and registers ALL validators (e.g., `LoginRequestValidator`, `CreateGroupRequestValidator`, etc.) in the DI container

**No manual registration needed.** All validators in the assembly are auto-discovered.

### 2. Request/Validator Pairing

**Convention:** Validator class name must match DTO type name + "Validator"

| DTO | Validator Class | File Path |
|-----|-----------------|-----------|
| `LoginRequest` | `LoginRequestValidator` | `src/SoberNetwork.Core/Validators/Auth/LoginRequestValidator.cs` |
| `RegisterRequest` | `RegisterRequestValidator` | `src/SoberNetwork.Core/Validators/Auth/RegisterRequestValidator.cs` |
| `CreateGroupRequest` | `CreateGroupRequestValidator` | `src/SoberNetwork.Core/Validators/Groups/CreateGroupRequestValidator.cs` |
| `CreateMeetingRequest` | `CreateMeetingRequestValidator` | `src/SoberNetwork.Core/Validators/Groups/CreateMeetingRequestValidator.cs` |

### 3. Controller Action (No Manual Validation)

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request,  // Automatically validated!
    CancellationToken cancellationToken = default
)
{
    // If we reach this line, request is GUARANTEED to be valid
    // ValidationException is never thrown here
    
    var result = await _mediator.Send(new LoginQuery(request.Email, request.Password), cancellationToken);
    return result.ResultCode switch
    {
        ResultCode.Success => Ok(result.Data),
        ResultCode.Unauthorized => Unauthorized(new ProblemDetails { ... }),
        _ => BadRequest(...)
    };
}
```

**Key point:** The validator runs BEFORE this method is called. If validation fails, the middleware returns 400 and this method never executes.

---

## Example: Complete Flow

### 1. Define the DTO (Request)

```csharp
// src/SoberNetwork.Core/DTOs/Auth/LoginRequest.cs
namespace SoberNetwork.Core.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password
);
```

### 2. Define the Validator

```csharp
// src/SoberNetwork.Core/Validators/Auth/LoginRequestValidator.cs
using FluentValidation;
using SoberNetwork.Core.DTOs.Auth;

namespace SoberNetwork.Core.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(256)
            .WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MaximumLength(256)
            .WithMessage("Password cannot exceed 256 characters.");
    }
}
```

### 3. Controller Action (No Manual Validation)

```csharp
// src/SoberNetwork.Api/Controllers/AuthController.cs
[HttpPost("login")]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request,
    CancellationToken cancellationToken = default
)
{
    // Validation AUTOMATICALLY ran before this line
    // If request was invalid, a 400 was already sent and this never executes
    
    var result = await _mediator.Send(
        new LoginQuery(request.Email, request.Password),
        cancellationToken
    );
    
    return result.ResultCode switch
    {
        ResultCode.Success => Ok(result.Data),
        ResultCode.Unauthorized => Unauthorized(...),
        _ => BadRequest(...)
    };
}
```

### 4. Client Sends Invalid Request

**Request:**
```json
POST /api/auth/login
{
  "email": "not-an-email",
  "password": ""
}
```

**Automatic Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "email": [
      "Email must be a valid email address."
    ],
    "password": [
      "Password is required."
    ]
  }
}
```

**No code in the controller does this.** The middleware handles it automatically.

---

## Key Rules

### File Structure Convention

```
src/SoberNetwork.Core/
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   └── RegisterRequest.cs
│   ├── Groups/
│   │   ├── CreateGroupRequest.cs
│   │   └── UpdateGroupRequest.cs
│   └── Members/
│       └── UpdateProfileRequest.cs
└── Validators/
    ├── Auth/
    │   ├── LoginRequestValidator.cs         ← Validates LoginRequest
    │   └── RegisterRequestValidator.cs      ← Validates RegisterRequest
    ├── Groups/
    │   ├── CreateGroupRequestValidator.cs   ← Validates CreateGroupRequest
    │   └── UpdateGroupRequestValidator.cs   ← Validates UpdateGroupRequest
    └── Members/
        └── UpdateProfileRequestValidator.cs ← Validates UpdateProfileRequest
```

**Naming Convention:**
- DTO: `{Action}{Entity}Request` (e.g., `LoginRequest`, `CreateGroupRequest`)
- Validator: `{Action}{Entity}RequestValidator` (e.g., `LoginRequestValidator`, `CreateGroupRequestValidator`)

### Validator Inheritance

All validators must inherit from `AbstractValidator<TDto>`:

```csharp
public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.Slug)
            .NotEmpty()
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug may only contain lowercase letters, numbers, and hyphens.");
    }
}
```

---

## Special Cases

### GET Requests with Query Parameters

GET requests with `[FromQuery]` parameters ALSO get validated:

```csharp
public record PaginationQuery(
    int Page = 1,
    int PageSize = 10
);

public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
```

**Controller:**
```csharp
[HttpGet("members")]
public async Task<IActionResult> GetMembers(
    [FromQuery] PaginationQuery pagination,  // Auto-validated!
    CancellationToken cancellationToken = default
)
{
    // If we're here, pagination.Page >= 1 and pagination.PageSize in [1, 100]
    ...
}
```

### Route Parameters (Guid)

Guid model binding is handled automatically:

```csharp
[HttpGet("{userId:guid}")]
public async Task<IActionResult> GetUser(
    Guid userId,  // Model binding converts string route param to Guid
    CancellationToken cancellationToken = default
)
{
    // If route was `/api/users/not-a-guid`, ASP.NET returns 400 automatically (no validator needed)
    ...
}
```

---

## Testing Validation

### Unit Test Example

```csharp
public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyEmail_ReturnsFalse()
    {
        // Arrange
        var request = new LoginRequest(Email: "", Password: "password");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Validate_WithInvalidEmail_ReturnsFalse()
    {
        var request = new LoginRequest(Email: "not-an-email", Password: "password");
        var result = _validator.Validate(request);
        
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithValidRequest_ReturnsTrue()
    {
        var request = new LoginRequest(Email: "user@example.com", Password: "password");
        var result = _validator.Validate(request);
        
        Assert.True(result.IsValid);
    }
}
```

### Integration Test Example

```csharp
[Fact]
public async Task Login_WithInvalidEmail_Returns400()
{
    // Arrange
    var request = new { email = "not-an-email", password = "password" };
    var content = JsonContent.Create(request);

    // Act
    var response = await _client.PostAsync("/api/auth/login", content);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.Contains("email", body, StringComparison.OrdinalIgnoreCase);
}
```

---

## Benefits of This Pattern

| Benefit | Why It Matters |
|---------|----------------|
| **No boilerplate** | Controllers stay clean — no `if (!ModelState.IsValid)` checks |
| **Consistent responses** | All validation errors follow the same ProblemDetails format |
| **Testable** | Validators are simple classes that can be tested independently |
| **Discoverable** | Auto-registration means new validators are picked up automatically |
| **Separation of concerns** | Validation logic is in validators, business logic stays in handlers |
| **Reusable** | The same validator can validate requests from multiple endpoints |

---

## When to Create a Validator

✅ **Create a validator when:**
- A DTO accepts user input via `[FromBody]` or `[FromQuery]`
- You need format validation (email, URL, regex patterns)
- You have business rules (min/max lengths, required fields)
- Multiple endpoints share the same DTO

❌ **Don't create a validator for:**
- Simple type validation (Guid, int) — ASP.NET model binding handles this
- Domain validation (e.g., "user can't join the same group twice") — handle in handlers/services
- Cross-cutting concerns (auth, permissions) — use `[Authorize]` attributes or middleware

---

## Common Validators in This Codebase

```csharp
// Auth
LoginRequestValidator       → Email (valid), Password (not empty)
RegisterRequestValidator    → Email (valid), Password (strong), DisplayName (not empty)
ForgotPasswordRequestValidator → Email (valid)

// Groups
CreateGroupRequestValidator → Name (not empty), Slug (lowercase + hyphens), TimeZone (valid)
UpdateGroupRequestValidator → Same as Create but fields are optional
CreateMeetingRequestValidator → Title (not empty), Type (valid), Formats (valid), 
                                 Address fields (required if InPerson/Hybrid)

// Members
UpdateProfileRequestValidator → DisplayName (max 100), SobrietyDate (not future)
SetPhoneRequestValidator → Phone (valid format)
ChangePasswordRequestValidator → CurrentPassword + NewPassword (strong + different)

// Common
PaginationQueryValidator → Page (>= 1), PageSize (1-100)
```

---

## Troubleshooting

**Q: My validator isn't being called. Why?**
- Make sure it's in `SoberNetwork.Core.Validators` namespace
- Check naming: must be `{DtoName}Validator` for auto-discovery
- Ensure the validator class inherits from `AbstractValidator<TDto>`
- Rebuild solution — sometimes DI container needs rebuild

**Q: I'm getting 400 but my error message is generic. How do I customize it?**
```csharp
RuleFor(x => x.Email)
    .EmailAddress()
    .WithMessage("Please provide a valid email address.");
```

**Q: Can I disable validation for a specific endpoint?**
```csharp
[HttpPost("public-endpoint")]
[AllowAnonymous]  // Different attribute, but validation still runs
public IActionResult PublicEndpoint([FromBody] MyRequest request)
{
    // Validation still happens — you can't disable it per-endpoint
    // Workaround: create a separate DTO without validators, or use a different validation approach
}
```

**Q: My DTO has nested objects. How do I validate them?**
```csharp
public record CreateGroupRequest(
    string Name,
    Address Address  // Nested object
);

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        
        // Validate nested object
        RuleFor(x => x.Address).SetValidator(new AddressValidator());
    }
}

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
    }
}
```

---

## Summary

**Validation auto-response** = Automatic request validation at the HTTP boundary with zero controller code.

The pattern:
1. Create a DTO (e.g., `LoginRequest`)
2. Create a matching validator (e.g., `LoginRequestValidator : AbstractValidator<LoginRequest>`)
3. Define rules in the validator constructor
4. ASP.NET automatically validates incoming requests against the validator
5. Invalid requests return **400 Bad Request** with error details
6. Only valid requests reach your controller

No manual validation code needed. It just works. ✨
