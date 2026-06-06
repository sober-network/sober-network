# Sober Network — Copilot Standing Instructions

These instructions apply to every task, feature, PR, and code change on this repository.
They are non-negotiable. Always apply them when generating, reviewing, or refactoring code.

---

## 1. The 12 Traditions of A.A. — Design Constraints

The platform must honor the 12 Traditions at every design decision. Run this checklist
**before** implementing any new feature. If any answer is "yes" or "unclear", stop and
resolve it before writing code.

| # | Question | Tradition |
|---|----------|-----------|
| 1 | Does this expose any member data (name, email, phone, sobriety date) to unauthenticated users? | T11, T12 |
| 2 | Does this create a public-facing profile, roster, or searchable directory of members? | T11, T12 |
| 3 | Does this require personal information beyond what's strictly necessary for the feature? | T3 |
| 4 | Could this give one member disproportionate power or visibility over others? | T2, T9 |
| 5 | Could this allow one group's data to be seen by another group or its members? | T4 |
| 6 | Does this serve the primary purpose of recovery, or does it distract from it? | T5 |
| 7 | Does this involve advertising, sponsorship, gamification, or engagement mechanics? | T5, T6 |
| 8 | Does this involve or imply professional clinical services (therapy, counseling)? | T8 |
| 9 | Could this draw the platform into political or social controversy? | T10 |
| 10 | Does this use the name or trademarks of Alcoholics Anonymous? | T6 |

**Key tradition implications:**
- **T2** — `group_admin` is a trusted servant, not a ruler. Platform must support group conscience decisions.
- **T3** — Sobriety date, phone, last name, location are all opt-in. Never required.
- **T4** — Groups are autonomous. No cross-tenant data leakage. `superadmin` intervenes only when a group affects others.
- **T5** — Features must serve recovery. No advertising, gamification, or engagement-bait mechanics.
- **T6** — Platform is named **Sober Network**, never "AA [anything]". No AA logos or trademarks.
- **T7** — No outside donations, VC funding, or sponsorships.
- **T11/T12** — No public member roster, no public profiles, no searchable member directory. Display names default to first name only. Anonymity is a first-class architectural constraint.

---

## 2. Security Principles


- **`[Authorize]` is the default** on all controllers. `[AllowAnonymous]` must be explicitly applied AND must have a comment explaining why it's public and that it passed T11/T12 review.
- All users must be authenticated before accessing member content.
- Phone lists and member information are **never** exposed publicly.
- Public-facing content is strictly limited: meeting times, contact form — no member data.
- No user enumeration: auth endpoints always return generic error messages.
- Entities (`ApplicationUser`, `Group`, etc.) never leave the API layer. Always map to a DTO before returning.
- Input DTOs use **FluentValidation** for request validation at the boundary.
- Use **CancellationToken** in all async endpoints.

---

## 3. General C# Standards

- Use **C# 12** features when appropriate.
- Prefer **async/await** everywhere; avoid `Task.Run` unless explicitly required. Async all the way down — never generate synchronous I/O or blocking calls.
- Use **guard clauses** at the start of methods.
- Use **interfaces** for abstractions; avoid returning concrete types from public APIs.
- Prefer **records** for immutable models (DTOs, value objects).
- Use **expression-bodied members** when they improve clarity.
- Follow **PascalCase** for public members and **camelCase** for locals and parameters.
- Avoid static classes for business logic.
- Generate **XML documentation** for all public APIs.

---

## 4. Clean Architecture & SOLID

### Layer Responsibilities

| Layer | Project | Contains |
|-------|---------|----------|
| Domain | `SoberNetwork.Domain` | Entities, value objects, domain services, domain events |
| Application | `SoberNetwork.Core` | Commands, queries, MediatR handlers, validators, persistence interfaces |
| Infrastructure | `SoberNetwork.Infrastructure` | EF Core DbContext, repository implementations, external services |
| API | `SoberNetwork.Api` | Controllers, request/response DTOs, filters, middleware |

- **Domain never imports Application or Infrastructure** — enforced by project reference structure. **Application (Core) never imports Infrastructure.**
- Controllers and services depend on **interfaces**, never on concrete implementations.
- **Never bypass the Application layer** to access the database directly from a controller.
- Domain models must contain **business logic** — not DTOs or EF Core attributes.
- Use **MediatR** for all commands and queries. Never bypass MediatR for application logic.

### SOLID

**Single Responsibility** — Every class does one thing. No business logic in controllers; controllers orchestrate only.

**Open/Closed** — New behavior via new classes implementing existing interfaces, not by modifying working code.

**Liskov Substitution** — Any implementation of an interface must be fully substitutable.

**Interface Segregation** — Interfaces are small and focused (`ITokenService`, `IEmailService`, `IAuditService`). No God interfaces.

**Dependency Inversion** — Application layer defines interfaces; Infrastructure implements them; Api consumes via DI.

---

## 5. Dependency Injection

- All services must be registered via DI.
- Prefer **constructor injection** — avoid service locators or static access to services.
- Use **IOptions\<T\>** for configuration.
- Use **AddScoped** for business services unless a different lifetime is explicitly required.

---

## 6. EF Core Standards

- Use **DbContext** per request (scoped lifetime).
- Avoid lazy loading; prefer **explicit** or **eager** loading.
- Always use **AsNoTracking** for read-only queries.
- Avoid N+1 queries; use `Include`, `ThenInclude`, or projection.
- Use **migrations** for all schema changes — never modify the database manually.
- Use **value objects** where appropriate; avoid primitive obsession.
- Do not expose `IQueryable` from repositories.
- Repositories return domain models or DTOs, not EF entities.

---

## 7. API Design Standards

**URLs**
- Resources are plural nouns: `GET /api/groups`, `POST /api/groups/{slug}/members`
- Use **slugs** for group identifiers in URLs — never raw database IDs (T12)
- Nested routes for owned resources: `/api/groups/{slug}/events`
- Actions that don't fit REST get a verb: `/api/auth/refresh`

**HTTP Conventions**
- `GET` — read, never mutates
- `POST` — create or action
- `PUT` / `PATCH` — full / partial update
- `DELETE` — soft delete only (`deleted_at`), never removes rows

**Response Shape**
- Success: return the resource or a plain confirmation — never raw entity objects or EF entities
- Error: return **ProblemDetails** (`application/problem+json`) — never stack traces, never Identity error detail in production
- Auth endpoints always return generic messages — no user enumeration

---

## 8. Error Handling & Logging

- Do not swallow exceptions.
- Use structured logging with **ILogger\<T\>**.
- Use domain-specific exceptions only when meaningful; avoid throwing generic `Exception`.
- Logs contain `userId` (GUID) **only** — never email, display name, sobriety date, phone number, or IP in application logs (T12).
- IP and user agent are stored only in `security_events` for security purposes.
- `ClientLogData = Record<string, string>` — all Angular log metadata values must be strings; convert with `String()` at the call site.
- Audit log rows describe *actions*, not *people*.

---

## 9. Data Standards

**Every table must have:**
- `created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()`
- `updated_at TIMESTAMPTZ` (via EF `SaveChanges` interceptor)
- `deleted_at TIMESTAMPTZ NULL` — soft delete only, never hard delete member content

**Multi-tenancy (T4)**
- Every table with member-scoped data must have a `group_id` FK to `groups`
- Queries for member data must always include a `group_id` filter — no cross-tenant leakage ever

**Naming**
- PostgreSQL: `snake_case` — C# entities: `PascalCase`
- EF Core maps via `UseSnakeCaseNamingConvention()`

**Indexes**
- Always index foreign keys
- Index columns used in `WHERE` clauses: `group_id`, `user_id`, `created_at`, `slug`

---

## 10. Testing Standards

- Use **xUnit** for all tests.
- Use **Moq** or **NSubstitute** for mocking.
- Follow **AAA (Arrange-Act-Assert)** structure.
- Unit tests must not depend on EF Core or external services.
- Integration tests may use **Testcontainers** or an in-memory database.

**Security test requirements — every protected endpoint must have:**
1. A test asserting unauthenticated requests return `401`
2. A test asserting insufficient role returns `403`
3. A test asserting cross-group access returns `403` or `404` (no data leakage)

**Test naming convention:**
```
MethodName_Scenario_ExpectedResult
// e.g.:
GetPhoneList_AsNonMember_ReturnsForbidden
Login_WithInvalidPassword_ReturnsUnauthorized
```

**No production secrets in tests.** Use in-memory SQLite or a dedicated test schema.

---

## 11. Code Generation Preferences

When generating code:
- Prefer **clean, minimal, readable** solutions. Avoid unnecessary abstractions.
- Use **async all the way down** with `CancellationToken` in all async methods.
- Use **dependency injection** patterns consistently.
- Generate **XML documentation** for all public APIs.
- Generate **DTOs** for all API input/output — never expose domain models or EF entities directly.
- Follow architecture boundaries strictly — place new files in the correct layer.

---

## 12. File & Folder Structure

```
/Domain (SoberNetwork.Domain)      — Entities, value objects, domain services, events
/Application (SoberNetwork.Core)   — Commands, queries, MediatR handlers, validators, interfaces
/Infrastructure                    — EF Core DbContext, repository implementations, external services
/API (SoberNetwork.Api)            — Controllers, DTOs, filters, middleware
```

> **Note:** `ApplicationUser` inherits `IdentityUser` — Domain holds an intentional Microsoft.AspNetCore.Identity dependency as an accepted architectural compromise. A future refactor may introduce a pure domain user type.

Place new files in the correct layer automatically.

---

## 13. Architecture Reminders

- **Modular Monolith** — clean module boundaries: `Auth`, `Groups`, `Events`, `Documents`, `Members`, `Notifications`
- `IsPublic` on `Group` controls directory discoverability only — NOT direct-link access. Private groups can still share join links.
- `group_admin` is a trusted servant role — succession planning is a platform feature, not an afterthought.

---

## 14. Things Copilot Must Never Do

- Never access DbContext from controllers.
- Never put business logic in controllers.
- Never return EF Core entities from API endpoints.
- Never create static helper classes for domain logic.
- Never bypass MediatR for application logic.
- Never generate synchronous I/O or blocking calls.
- Never hard-delete rows — always use soft delete (`deleted_at`).
- Never expose member data (email, phone, sobriety date, full name) to unauthenticated users.
- Never add `[AllowAnonymous]` without a comment explaining the T11/T12 review.
- Never cross group boundaries in a query — always filter by `group_id`.

---

## 15. Build & Validation Commands

### Building

```powershell
# Build backend API
dotnet build src\SoberNetwork.Api\SoberNetwork.Api.csproj

# Build frontend
cd src\SoberNetwork.Web && npx ng build --configuration=development
```

### Testing

```powershell
# Run all backend tests
dotnet test tests\SoberNetwork.Core.Tests\SoberNetwork.Core.Tests.csproj
dotnet test tests\SoberNetwork.Api.Tests\SoberNetwork.Api.Tests.csproj

# Run a single test class or method (xUnit filter syntax)
dotnet test tests\SoberNetwork.Core.Tests\SoberNetwork.Core.Tests.csproj --filter "GetPhoneListQueryHandlerTests"

# Run frontend tests
cd src\SoberNetwork.Web && npm test
```

### Test Infrastructure

**API Integration Tests** use `TestWebApplicationFactory` (tests\SoberNetwork.Api.Tests\Infrastructure\):
- Creates an authenticated client: `factory.CreateAuthenticatedClient(userId)`
- Creates a superadmin client: `factory.CreateSuperAdminClient(userId)`
- Exposes mocked services: `factory.MockGroupService`, `factory.MockMemberService`, etc.
- Generate test JWTs: `JwtTestHelper.GenerateToken(userId, claims)`

**Backend tests** follow:
- **File structure:** Handlers in `/tests/SoberNetwork.Core.Tests/Handlers/{Feature}`, Validators in `/tests/SoberNetwork.Core.Tests/Validators/{Feature}`
- **AAA pattern:** Arrange-Act-Assert
- **Naming:** `MethodName_Scenario_ExpectedResult` (e.g., `GetPhoneList_AsNonMember_ReturnsForbidden`)

**Result-mapping pattern:** String-contains checks on error messages select ResultCode→HTTP status:
- "permission" / "not a member" → `Forbidden`
- "already" / "only admin" → `Conflict`
- "Incorrect" → `Unauthorized`
- "match" → `BadRequest`

Always run both backend and frontend tests after making changes.

---

## 16. Key Architecture Patterns & Module Organization

### Module Boundaries

The platform uses a modular monolith with clean separation:
- **Auth** — Login, registration, password reset, JWT token lifecycle
- **Groups** — Group CRUD, member management, joining/leaving, admin operations
- **Members** — Individual member profiles, account settings, sobriety data
- **Events** — Group events/meetings (CRUD, search, public directory)
- **Documents** — Group-owned documents and resources
- **Notifications** — Email and in-app notification delivery
- **Security** — Audit logging, superadmin operations

### Handler Query Pattern

Commands and Queries live in `/src/SoberNetwork.Core/Handlers/{Feature}/`:
- One file per handler (e.g., `GetPhoneListQueryHandler.cs`)
- All commands/queries return `Result<T>` with `ResultCode` (Success, Unauthorized, Forbidden, BadRequest, Conflict, etc.)
- Handlers **never** return raw domain entities — always map to DTOs
- Controllers switch on `result.ResultCode` to return HTTP status

### DTO Mapping Convention

- Request DTOs: `{Action}{Entity}Request` (e.g., `CreateGroupRequest`, `UpdateMemberRequest`)
- Response DTOs: `{Entity}Response` or `{Entity}Dto` (e.g., `GroupResponse`, `MemberDto`)
- All validation via **FluentValidation** at the API boundary (not in handlers)
- Mapping: Handlers map domain models to DTOs before returning via `Result<T>`

### Multi-Tenancy (Group Isolation)

Every query for member-scoped data **must** filter by `group_id`:
```csharp
var members = await _context.Members
    .Where(m => m.GroupId == groupId)  // REQUIRED — no cross-tenant leakage
    .AsNoTracking()
    .ToListAsync(cancellationToken);
```
Violations are security issues (T4). Test with `CrossGroupAccessTests`.

### Entity Soft Deletes

- All entities have `deleted_at: TIMESTAMPTZ NULL`
- `DELETE` endpoints set `deleted_at` — never hard-delete rows
- Queries use `.Where(e => e.DeletedAt == null)` or a `IsDeleted` extension method

### Platform Stats & Public Data

Public endpoints (marked `[AllowAnonymous]`):
- `GET /api/stats` — Platform statistics (MemberCount, GroupCount, MeetingCount) via `StatsService`
- `GET /api/meetings` — Public meeting finder (Haversine distance, Nominatim geocoding, Leaflet maps)
- `POST /api/auth/register`, `POST /api/auth/login` — Auth endpoints (generic error messages, no user enumeration)
- Group join links — shareable but not listed in directory unless `IsPublic = true`

---

## 17. UI Design System (Apply Sitewide)

The approved visual design is captured in `docs/knowledge.md § UI Design System` and the v1.3 HTML mockup. **All Angular components and pages must follow this design system.** When implementing any Angular component, consult the mockup and apply these rules consistently.

### Fonts
- **Inter Tight** — headings, nav, labels (weights 400–900)
- **Instrument Serif italic** — display accent lines inside headings
- **Inter** — body copy
- Always load via Google Fonts

### Color Palette
- `--bg: #f7f5f2` · `--bg-soft: #f0ede8` · `--text: #111110` · `--text-mid: #3d3d3a` · `--text-muted: #7a7a75`
- Dark CTA: `--cta-bg: #1a1a18`
- Sections alternate between `white`, `--bg`, and `--cta-bg` (see knowledge.md for full cadence)

### Components
- **Nav**: white card pill container; active pill = solid black; rainbow conic-gradient logo ring; gradient accent underline
- **Feature cards / icon bubbles**: 8 cheerful tint colors (violet, rose, sky, amber, green, teal, indigo, coral at 12% opacity)
- **Badges/pills**: same 8 tints applied to category labels and marquee icons
- **Buttons**: dark gradient pill (primary); ghost outline (secondary); frosted arrow-circle icon on CTAs
- **Back-to-top buttons**: black circle, `position: absolute` anchored to `.section-wrap` (NOT `section`) at `top: 80px; right: 32px`
- **Sections**: `max-width: 1200px`, `padding: 100px 32px`, `position: relative` on `.section-wrap`

### Images
- **Real photos**: background-image behind gradient overlay at ~12–15% opacity (`mix-blend-mode` not needed)
- **Vector art with white/off-white bg**: `mix-blend-mode: multiply` + `filter: brightness(1.35) contrast(1.05)` to remove backgrounds cleanly
- Free assets from `static.vecteezy.com` via `non_2x` preview URLs

### Hero Section
- Gradient overlay (sky→peach) + photo background
- Large Inter Tight headline (weight 900, `letter-spacing: -0.04em`) with one Instrument Serif italic accent line
- Centered eyebrow pill, CTA buttons, trust strip
- Hero `font-size: clamp(3rem, 6.5vw, 5.6rem)`

### Scroll Behaviour
- `IntersectionObserver` scroll-spy on nav pills (`rootMargin: '-10% 0px -55% 0px'`)
- Fade-up entrance animation (`.fade-up` → `.visible` via observer, `threshold: 0.1`)
- Staggered delays: `.delay-1`, `.delay-2`, `.delay-3`
