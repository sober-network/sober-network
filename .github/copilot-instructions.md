# Sober Network — Copilot Standing Instructions

These instructions apply to every task, feature, PR, and code change on this repository.
They are non-negotiable. Always apply them when generating, reviewing, or refactoring code.

---

## Project Overview & Tech Stack

**Sober Network** is a multi-tenant web platform for A.A. home groups. Anonymity (T11/T12) and group
autonomy (T4) are first-class architectural constraints. **§1 (The 12 Traditions) is the highest-priority
rule in this document — run its checklist before any feature work.**

| Area | Technology |
|------|-----------|
| Backend | ASP.NET Core **.NET 10** (`net10.0`, C# 14), Clean Architecture across 4 projects (see §12) |
| Application layer | **MediatR** 12 (commands / queries / handlers) + **FluentValidation** 11 |
| Data | **PostgreSQL 16** via EF Core 10 + **Npgsql**, snake_case naming convention |
| Auth | ASP.NET **Identity** (`Guid` keys) + **JWT Bearer**; a global `[Authorize]` fallback policy (see §23) |
| Email / Geocoding | **Resend** (transactional email) · **Nominatim** / OpenStreetMap (geocoding) |
| Logging | **Serilog** → console + rolling file `logs/app-.log` |
| Frontend | **Angular 21** (standalone components, Angular Material, Leaflet maps), TypeScript 5.9 |
| Frontend tests | **Vitest** (`@angular/build:unit-test` builder) |
| Hosting | Multi-stage **Docker** image → **Fly.io**; GitHub Actions CI (`.github/workflows/deploy.yml`) |

**Canonical deep-dive doc:** `docs/knowledge.md` — project vision, the full 12-Traditions text with design
implications, architecture decisions, meeting-finder + platform-stats design, and the UI design system. Treat
it as the source of truth for product and design intent.

**Run locally** (full detail in §21): `docker compose up -d` starts Postgres → set user-secrets → start the
`SoberNetwork.Api` project, which applies EF migrations, seeds the superuser, and auto-starts the Angular dev
server (`ng serve`, http://localhost:4200) via the SPA proxy.

Build & test commands live in **§15**; deployment in **§22**; the `Program.cs` middleware/bootstrap wiring that
enforces several conventions below is documented in **§23**.

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

- Target framework is **`net10.0`** (C# 14 is available) — use modern C# features when they improve clarity.
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

**userId Type: Guid**
- `ApplicationUser : IdentityUser<Guid>` — all Identity tables store uuid columns
- `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- **JWT boundary rule (firm):** JWT claims are `string` by spec. Conversion points only:
  - `TokenService`: `new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())`
  - Controllers: `private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`
- `UserManager<TUser>` methods (`FindByIdAsync`, etc.) still take `string` — call `.ToString()` at those call sites only
- `IAuditService.LogAsync` takes `Guid? userId` — callers pass Guid directly
- `ResetPasswordRequest.UserId` is `Guid` — ASP.NET JSON binding handles string→Guid automatically
- `[FromQuery] Guid userId` — model binding handles string→Guid conversion
- Moq setups for userId args: `It.IsAny<Guid>()` not `It.IsAny<string>()`
- Test URLs with userId in route segments must use valid Guid strings (e.g., `"00000000-0000-0000-0000-000000000001"`) or model binding returns 400
- `TestWebApplicationFactory.CreateAuthenticatedClient(Guid userId = default)` — `default(Guid)` is `Guid.Empty` (valid)

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

**Exceptions to (2) and (3):**
- **Self-scoped endpoints** (`/api/members/me/*`) derive identity solely from the JWT `NameIdentifier` claim and only ever touch the caller's own data — there is no role or cross-group dimension, so only the `401` test applies.
- **Global superadmin endpoints** (`GET /api/members`, `/api/members/{userId}`, deactivate) are platform-wide, not group-scoped — they require `401` + `403`, but cross-group does not apply.
- All `[AllowAnonymous]` endpoints are exempt from `401`.

**Test naming convention:**
```
MethodName_Scenario_ExpectedResult
// e.g.:
GetPhoneList_AsNonMember_ReturnsForbidden
Login_WithInvalidPassword_ReturnsUnauthorized
```

**No production secrets in tests.** Use in-memory SQLite or a dedicated test schema.

---

## 10b. End-to-End Testing with Playwright

**E2E tests** validate complete user workflows across frontend and backend. Use [Playwright](https://playwright.dev/) for browser automation.

### Running E2E Tests

```powershell
cd src\SoberNetwork.Web

# Run all tests (headless)
npm run e2e

# Interactive UI mode — **recommended for development**
npm run e2e:ui

# Debug mode — step through tests
npm run e2e:debug

# View HTML report
npm run e2e:report

# Single browser or mobile
npx playwright test --project=chromium
npx playwright test --project="Mobile Chrome"

# Single file or test by name
npx playwright test e2e/landing.spec.ts
npx playwright test -g "Auth Flow"
```

### Test Structure

Tests live in `src/SoberNetwork.Web/e2e/`:
- **`landing.spec.ts`** — Landing page, hero, header
- **`auth.spec.ts`** — Login/register modals, form validation
- **`e2e/README.md`** — Full guide on writing E2E tests

### Writing E2E Tests

**Template:**
```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test('should do X when Y happens', async ({ page }) => {
    // Arrange
    await page.goto('/');
    
    // Act
    await page.locator('button', { hasText: /Sign In/i }).click();
    
    // Assert
    await expect(page.locator('[role="dialog"]')).toBeVisible();
  });
});
```

### Best Practices

- Use **semantic locators** — `[role="dialog"]`, `button`, `[aria-label]` over CSS classes
- Use **`{ hasText: /pattern/i }`** for accessibility (case-insensitive text matching)
- **Don't test implementation details** — test user workflows
- **One assertion per section** — use `test.describe` for grouping
- **Playwright waits automatically** — up to 30s for element visibility
- **Mobile testing included** — tests run on Pixel 5, iPhone 12 by default

### Configuration

`playwright.config.ts`:
- Auto-starts `npm start` (dev server) before tests
- Runs on Chrome, Firefox, Safari, and mobile viewports
- Captures screenshots on failure
- Traces on first retry (helpful for debugging)

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

# Run frontend tests (Angular 21 — Vitest runner via @angular/build:unit-test)
cd src\SoberNetwork.Web && npm test            # all specs (ng test)

# Run a single frontend spec or a test by name (Vitest CLI)
cd src\SoberNetwork.Web && npx vitest run src\app\path\to\thing.spec.ts
cd src\SoberNetwork.Web && npx vitest run -t "renders the login form"

# Run E2E tests (Playwright — requires dev server running)
cd src\SoberNetwork.Web && npm run e2e        # headless
cd src\SoberNetwork.Web && npm run e2e:ui     # interactive UI mode (recommended)
cd src\SoberNetwork.Web && npm run e2e:debug  # step through with debugger
```

> Run each test project separately (as above). Solution-level `dotnet test` fails because
> `SoberNetwork.Web.esproj` is part of the solution.

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

### Entity-Specific Patterns

**Meeting Entity**
- First-class entity with its own table, service (`IMeetingService/MeetingService`), controller (`MeetingsController`), and Angular components
- Fields: `Id`, `GroupId`, `Title`, `Description`, `MeetingType` (InPerson=0, Online=1, Hybrid=2), `TimeBlock` (Morning, Afternoon, Evening, Night)
- Address fields (for InPerson/Hybrid): `VenueName`, `Street`, `City`, `State`, `PostalCode`, `Country`, `Lat`, `Lon`
- Meeting formats: `Formats` is `text[]` (PostgreSQL array), NOT comma-separated string. Canonical enum-style values (15 total): `Discussion`, `Speaker`, `StepStudy`, `TraditionStudy`, `BigBook`, `Literature`, `Topic`, `Beginners`, `Candlelight`, `Meditation`, `BirthdayChip`, `Men`, `Women`, `YoungPeople`, `LGBTQPlus`. Stored as the canonical token; the human-readable label (e.g. `StepStudy` → "Step Study", `BirthdayChip` → "Birthday / Chip", `LGBTQPlus` → "LGBTQ+") is resolved on the frontend via `MEETING_FORMAT_LABELS` / `formatMeetingFormat()` in `core/models/group.models.ts`. Backend `CreateMeetingRequestValidator`/`UpdateMeetingRequestValidator` validate against the same canonical set.
- URLs: `ZoomLink` (members-only, NEVER exposed publicly), `PublicJoinUrl` (safe, admin-curated, used in public meeting finder)
- `DaysOfWeek` as bitmask for recurring meetings (Monday=1, Tuesday=2, Wednesday=4, Thursday=8, Friday=16, Saturday=32, Sunday=64)
- Query validation: If `MeetingType` is InPerson or Hybrid, City/Street are **required**; if Online, they are **optional**
- Soft delete: `deleted_at` never exposed; queries include `.Where(m => m.DeletedAt == null)`
- Permissions: Only `group_admin` can create/edit/delete meetings; authenticated members can view their group's meetings; public endpoint shows only public meetings from public groups

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
- Commands and permission-gated queries return `Result<T>` / `DataResult<T>` / `CommandResult` with a `ResultCode` (Success, Unauthorized, Forbidden, BadRequest, Conflict, etc.); controllers switch on `ResultCode` → HTTP status
- **Read-query exception:** simple read queries that cannot fail with a domain/permission error (e.g., `GetPlatformStats`, `GetMyGroups`, `GetAllGroups`, `GetMyProfile`, `GetUserById`, `GetMailingAddress`, `SearchPublicMeetings`) may return the DTO/collection directly (or `null` for not-found, which the controller maps to `404`/`NoContent`). This is an accepted convention, not a violation.
- Handlers **never** return raw domain entities — always map to DTOs

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

**Platform Statistics Endpoint**
- `GET /api/stats` [AllowAnonymous] — Returns `PlatformStatsResponse(MemberCount, GroupCount, MeetingCount)`
- Member count = distinct `UserId` in `GroupMemberships` where `Status=Active` and `DeletedAt=null`
- Group count = `Groups` where `DeletedAt=null`
- Meeting count = `Meetings` where both meeting and its group have `DeletedAt=null`
- Backend: `IStatsService/StatsService` (Infrastructure) with three `AsNoTracking` COUNT queries
- Query: `GetPlatformStatsQuery` → `GetPlatformStatsQueryHandler` → `IStatsService`
- Angular: `StatsService` loads on `LandingComponent.ngOnInit()`; errors silently fall back to zero

**Public Meeting Finder**
- `GET /api/meetings` [AllowAnonymous] — Cross-group public meeting search (Haversine distance, Nominatim geocoding)
- Only groups with `IsPublic = true` appear in results
- `SearchPublicMeetingsQuery` / `SearchPublicMeetingsQueryHandler` — Haversine with bounding-box pre-filter
- Earth radius: 3958.8 miles; default search radius: 25 miles
- `Meeting` entity fields: `MeetingType`, `VenueName`, `Street`, `City`, `State`, `PostalCode`, `Country`, `Lat`, `Lon`, `PublicJoinUrl`
- `ApplicationUser` mailing address fields: `MailingStreet`, `MailingCity`, `MailingState`, `MailingPostalCode`, `MailingCountry`, `MailingLatitude`, `MailingLongitude` (all opt-in)
- Angular: `MeetingFinderComponent` — day chips, time-block and format filters, GPS geocoding address input, Leaflet map with markers, list view
- "Get Directions" links to Google Maps; "Join Online" opens `PublicJoinUrl` only for Online/Hybrid meetings with URL set
- Route: `/meetings` (no auth guard)

**Other Public Endpoints**
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
- **In the Angular app these tokens are `--sn-*` prefixed** (`--sn-bg`, `--sn-cta-bg`, … in `styles.scss :root`). The unprefixed names above describe the standalone HTML mockup. In components **always** reference the prefixed token with a literal fallback — `var(--sn-cta-bg, #1a1a18)` — never `var(--cta-bg)`, which is undefined, silently dropped, and previously produced invisible modal buttons.

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

---

## 18. Angular-Specific Patterns

### Form Control Typing
- Use `FormControl<T>` with explicit type parameter to avoid TS4111 binding errors
- Always pass `{ validators, nonNullable: true }` in constructor for typed controls
- Template accesses controls via `form.get('name')` instead of `form.controls.name` to maintain type safety
- Example:
  ```typescript
  this.meetingForm = new FormGroup({
    title: new FormControl<string>('', { 
      validators: Validators.required, 
      nonNullable: true 
    })
  });
  ```

### Modal Dialog Patterns
- Use `MatDialog` to open modals; pass `panelClass: ['sn-modal-panel', 'sn-<modal>-panel']` — the generic base (`sn-modal-panel`) strips Material's surface styling so the component owns visuals, and the second class is a **per-modal decorator hook**. Every modal has one — `sn-login-panel`, `sn-register-panel`, `sn-group-panel`, `sn-meeting-panel` — defined in `styles.scss`, where each modal's pane **width** lives (the `open()` config only sets `maxWidth: '100vw'`). The legacy `sn-form-modal` panel class has been removed.
- Global panel overrides in `styles.scss` remove Material defaults and apply custom radius/shadow
- Auth modals: `LoginModalComponent` and `RegisterModalComponent` cross-open via **dynamic imports** to avoid circular TypeScript dependencies
  ```typescript
  import('../../login-modal/login-modal.component').then(m => 
    this.dialog.open(m.LoginModalComponent, { panelClass: ['sn-modal-panel', 'sn-login-panel'] })
  );
  ```
- Modal success state closes dialog and navigates; error state keeps modal open showing validation errors
- Use `enterAnimationDuration: 0, exitAnimationDuration: 0` if animations cause timing issues

### Service Patterns
- `AuthService` handles all auth workflows (Register, Login, ForgotPassword, ResetPassword, Refresh, Logout)
- `logout()` navigates to `/` (landing page), NOT `/auth/login`
- Expired token flow: `APP_INITIALIZER` → `tryRestoreSession()` → 401 → `errorInterceptor` → `auth.logout()` → `/`
- Protected routes (`/dashboard`, `/groups`, etc.) redirect to `/auth/login` via `authGuard` when unauthenticated
- `StatsService` loads platform stats; errors silently fall back to zero to avoid blocking the UI
- Services should follow the `IService` interface pattern; use dependency injection via constructor

### Component Lifecycle
- Use `OnInit` for async data loading (queries, service calls)
- Use `ChangeDetectionStrategy.OnPush` with `ChangeDetectorRef.markForCheck()` in scroll-spy and observer callbacks
- Unsubscribe from observables in `OnDestroy` to prevent memory leaks (or use `takeUntilDestroyed(destroyRef)`)
- Wrap `IntersectionObserver` callbacks in `NgZone.run()` to ensure change detection runs

### Page Styling Convention
- Pages use `.page-container` (global class) with white background, `border-radius: 8px`, `padding: 2rem`
- Override in component SCSS if needed:
  ```scss
  .page-container {
    background: white;
    border-radius: 8px;
    padding: 2rem;
  }
  ```

### Routing
- Landing page: `/` (no auth guard, LandingComponent)
- Auth flows: `/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/reset-password/:token`
- Protected app routes: `/dashboard`, `/groups`, `/groups/:slug/*` (guarded by `authGuard`)
- Public routes: `/meetings` (meeting finder, no auth guard)
- Modal-driven auth (Sign In, Create Account) accessed from navbar; full-page routes remain for `authGuard` redirects and email confirmation links

---

## 19. Reusable Modal Component Pattern

The platform uses a **composition-based** modal system (NOT inheritance) for consistency and reusability. All form modals wrap their content in `<app-base-form-modal>` and use the same CSS class naming conventions.

### Architecture

**BaseFormModalComponent** (`src/SoberNetwork.Web/src/app/shared/components/base-form-modal/`)
- Provides the modal wrapper structure: header (icon, title, subtitle), close button, ng-content outlet
- Caps height at `90vh`; the projected content (`.lm-body`) scrolls while the header and close button stay fixed, so tall forms keep their action buttons reachable
- No business logic — purely structural and styling
- Used by composition: child components wrap their forms in `<app-base-form-modal>` tag

**Child Modal Components** (e.g., `LoginModalComponent`, `RegisterModalComponent`, `GroupFormModalComponent`, `MeetingFormModalComponent`)
- Standalone components that wrap their form in `<app-base-form-modal>`
- Each has its own template, styles, and form logic
- Do NOT extend BaseFormModalComponent (avoids inheritance gotchas with Angular templates)

### CSS Class Naming Convention

All modals use these classes consistently:

| Class | Purpose | Example |
|-------|---------|---------|
| `.lm-form` | Form container | `<form class="lm-form">` |
| `.lm-field` | Single form field wrapper | `<div class="lm-field">` |
| `.lm-label-row` | Label + counter row | `<div class="lm-label-row"><label>Name</label><span>0/100</span></div>` |
| `.lm-label` | Field label | `<label class="lm-label">Email</label>` |
| `.lm-input` | Text/email/date/time input | `<input class="lm-input" type="text" />` |
| `.lm-textarea` | Multi-line input | `<textarea class="lm-input lm-textarea"></textarea>` |
| `.lm-field-error` | Validation error message | `<div class="lm-field-error" *ngIf="...">Error text</div>` |
| `.form-section` | Logical section header | `<div class="form-section"><h4>Section Title</h4></div>` |
| `.error-message` | Form-level error alert | `<div class="error-message" *ngIf="error">{{ error }}</div>` |
| `.form-actions` | Button container | `<div class="form-actions">` |
| `.btn-primary` | Primary action button | `<button class="btn-primary">Save</button>` |
| `.btn-secondary` | Secondary/cancel button | `<button class="btn-secondary">Cancel</button>` |

### How to Create a New Modal

**Step 1: Create component directory**
```
src/SoberNetwork.Web/src/app/features/groups/new-form-modal/
  ├── new-form-modal.component.ts
  ├── new-form-modal.component.html
  └── new-form-modal.component.scss
```

**Step 2: Copy TypeScript template**
```typescript
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { GroupService } from '@app/core/services/group.service';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';

@Component({
  selector: 'app-new-form-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSlideToggleModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    BaseFormModalComponent,
  ],
  templateUrl: './new-form-modal.component.html',
  styleUrl: './new-form-modal.component.scss',
})
export class NewFormModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(GroupService);
  readonly dialogRef = inject(MatDialogRef<NewFormModalComponent>);
  private readonly data = inject(MAT_DIALOG_DATA) as { slug: string };

  form = this.fb.group({
    // Define your form controls here
  });

  title = 'Modal Title';
  subtitle = 'Modal subtitle';
  icon = 'edit';
  saving = false;
  error = '';

  ngOnInit(): void {
    // Initialize form with data if needed
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    // Call service, handle response
    this.saving = false;
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
```

**Step 3: Create HTML template (use class naming convention)**
```html
<app-base-form-modal [title]="title" [subtitle]="subtitle" [icon]="icon">
  
  <form [formGroup]="form" (ngSubmit)="save()" novalidate class="lm-form">
    
    <!-- Example field -->
    <div class="lm-field">
      <label class="lm-label">Field Name</label>
      <input 
        type="text"
        class="lm-input"
        formControlName="fieldName"
        placeholder="Placeholder text"
      />
      <div class="lm-field-error" *ngIf="form.get('fieldName')?.touched && form.get('fieldName')?.hasError('required')">
        This field is required.
      </div>
    </div>

    <!-- Error message -->
    <div class="error-message" *ngIf="error">{{ error }}</div>

    <!-- Action buttons -->
    <div class="form-actions">
      <button type="button" class="btn-secondary" (click)="cancel()" [disabled]="saving">
        Cancel
      </button>
      <button type="submit" class="btn-primary" [disabled]="saving || form.invalid">
        {{ saving ? 'Saving…' : 'Save' }}
      </button>
    </div>

  </form>

</app-base-form-modal>
```

**Step 4: Create SCSS with proven styling**
```scss
// ── Form styling ─────────────────────────────────────────────────────────
.lm-form { display: flex; flex-direction: column; gap: 16px; }

.lm-field { display: flex; flex-direction: column; gap: 6px; }

.lm-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--sn-text-mid, #3d3d3a);
  letter-spacing: 0.01em;
}

.lm-input {
  width: 100%;
  height: 44px;
  padding: 0 14px;
  border: 1px solid rgba(17,17,16,0.18);
  border-radius: 10px;
  background: rgba(255,255,255,0.7);
  font-family: 'Inter', sans-serif;
  font-size: 0.9375rem;
  color: var(--sn-text, #111110);
  outline: none;
  transition: border-color 0.15s, box-shadow 0.15s;

  &::placeholder { color: var(--sn-text-muted, #7a7a75); }
  &:focus {
    border-color: rgba(17,17,16,0.4);
    box-shadow: 0 0 0 3px rgba(17,17,16,0.06);
  }
}

.lm-field-error {
  font-size: 0.8rem;
  color: #c0392b;
}

.error-message {
  font-size: 0.875rem;
  color: #c0392b;
  padding: 8px 12px;
  background: rgba(192,57,43,0.07);
  border-radius: 8px;
}

// ── Action buttons ─────────────────────────────────────────────────────────
.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;

  button {
    flex: 1;
    height: 46px;
    border-radius: 100px;
    border: none;
    font-family: 'Inter Tight', sans-serif;
    font-size: 0.9375rem;
    font-weight: 600;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: opacity 0.15s;
    padding: 0;

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }

  .btn-primary {
    background-color: var(--sn-cta-bg, #1a1a18);
    color: #fff;
    &:hover:not(:disabled) { opacity: 0.88; }
  }

  .btn-secondary {
    background-color: transparent;
    color: var(--sn-text-mid, #3d3d3a);
    border: 1px solid var(--sn-text-muted, #7a7a75);
    &:hover:not(:disabled) { background-color: var(--sn-bg-soft, #f0ede8); }
  }
}
```

**Step 5: Open the modal from a parent component**
```typescript
import { MatDialog } from '@angular/material/dialog';

openNewModal(): void {
  this.dialog.open(NewFormModalComponent, {
    panelClass: ['sn-modal-panel', 'sn-newform-panel'],  // generic base + per-modal hook
    maxWidth: '100vw',
    data: { slug: this.slug },
    autoFocus: 'first-tabbable',
  });
}
// Pane width lives in the hook in styles.scss, e.g. .sn-newform-panel { width: 440px; }
```

### Key Principles

1. **Composition over inheritance** — wrap in `<app-base-form-modal>`, don't extend it
2. **CSS class convention** — always use `.lm-form`, `.lm-field`, `.lm-input` etc. for instant consistency
3. **panelClass: `['sn-modal-panel', 'sn-<modal>-panel']`** — generic base + a per-modal hook in `styles.scss` where the pane width and any future panel tweaks live
4. **No Material form fields** — use plain `<input>`, `<textarea>`, `<select>` with custom CSS instead
5. **Two buttons always** — Cancel (secondary) + Action (primary) in `.form-actions` div
6. **Error handling** — form-level errors in `.error-message`, field-level in `.lm-field-error`

### Reference Implementation

- **LoginModalComponent** — simplest example (email + password)
- **GroupFormModalComponent** — medium (multiple fields, toggles, sections)
- **MeetingFormModalComponent** — complex (checkboxes, conditionals, many fields)

Copy from one of these when creating a new modal. Time to create: **10 minutes** if following this pattern.

---

## 20. Database Migration Workflow

**Add a migration after entity changes:**
```powershell
dotnet ef migrations add MigrationName --project src\SoberNetwork.Infrastructure --startup-project src\SoberNetwork.Api
```

**Apply migrations to the database:**
```powershell
dotnet ef database update --project src\SoberNetwork.Infrastructure --startup-project src\SoberNetwork.Api
```

**Remove the last migration (before pushing):**
```powershell
dotnet ef migrations remove --project src\SoberNetwork.Infrastructure --startup-project src\SoberNetwork.Api
```

### Migration Requirements

- **All schema changes** must go through migrations — never modify the database manually
- Migrations are applied on app startup via `MigrateAsync()` in `Program.cs`
- **Every table must have:** `created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()`, `updated_at TIMESTAMPTZ`, `deleted_at TIMESTAMPTZ NULL`
- Use `HasDefaultValueSql("NOW()")` for timestamp defaults (not `HasDefaultValue(new DateTime(...))`)
- For array/collection column defaults, use `HasDefaultValueSql` (e.g., `HasDefaultValueSql("'{}'::text[]")` for PostgreSQL empty array)

### Superuser Setup & Secrets

**Set up user secrets before running the app:**
```powershell
dotnet user-secrets set "Superuser:Email" "admin@example.com" --project src\SoberNetwork.Api
dotnet user-secrets set "Superuser:Password" "YourSecurePassword123!" --project src\SoberNetwork.Api
dotnet user-secrets set "Superuser:DisplayName" "Platform Admin" --project src\SoberNetwork.Api
```

**Superuser seed block runs on every startup:**
- Skips if superuser already exists
- Creates user and assigns `superadmin` role
- Sets `EmailConfirmed = true`
- Reads from user secrets (`Superuser:Email`, `Superuser:Password`, `Superuser:DisplayName`)

### Database User Permissions

**PostgreSQL user requires CREATEDB privilege:**
```sql
ALTER USER sober CREATEDB;
```
- EF Core's `MigrateAsync` checks for DB existence and attempts `CREATE DATABASE`
- Requires `CREATEDB` even if the DB already exists
- Run this once during initial setup

### Development Workflow

1. Create entity or modify existing entity in `SoberNetwork.Domain/Entities/`
2. Update `DbContext` configuration in `SoberNetwork.Infrastructure/Data/Configurations/`
3. Run `dotnet ef migrations add MigrationName ...`
4. Review generated migration file — ensure naming conventions are correct (snake_case in SQL, PascalCase in C#)
5. Run `dotnet ef database update ...` to apply to local database
6. Test the changes locally
7. Commit migration file(s) to git
8. Verify migration applies cleanly after fresh clone: drop local DB, pull latest, run `dotnet ef database update`

### Common Issues

- **"No database provider found"** — ensure `--startup-project` points to `SoberNetwork.Api`
- **"PendingModelChangesWarning"** — rebuild solution after entity changes; EF Core caches the model
- **Migration already exists** — if two branches create overlapping migrations, rename one and check for conflicts in generated SQL
- **Timestamp columns** — always use `TIMESTAMPTZ` (timezone-aware) in PostgreSQL, never `TIMESTAMP`

---

## 21. Local Development Setup

**Prerequisites:** .NET 10 SDK · Node 22+ / npm 11 · Docker (for the local PostgreSQL container).

1. **Start the database** — `docker compose up -d` launches PostgreSQL 16 on `localhost:5432`
   (db `sobernetwork_dev`, user `sober` / `soberdev`). `docker compose down -v` wipes the volume for a clean
   reset. The `sober` user needs `CREATEDB` because EF's startup migration may create the database:
   `ALTER USER sober CREATEDB;` (see §20).
2. **Set user-secrets** on `SoberNetwork.Api` (never commit these): `Jwt:Secret`, `Resend:ApiKey`, and the
   `Superuser:Email` / `Superuser:Password` / `Superuser:DisplayName` seed values (see §20). The Postgres
   connection string is `ConnectionStrings:DefaultConnection` (in `appsettings.Development.json` or secrets).
3. **Run the API** — start the `SoberNetwork.Api` project (normally from your IDE/debugger). On boot it
   (a) applies pending EF migrations, (b) idempotently seeds the superuser, and (c) auto-starts the Angular dev
   server via `Microsoft.AspNetCore.SpaProxy` (`npm start` → `ng serve` on http://localhost:4200).
4. **Frontend only** (optional) — `cd src\SoberNetwork.Web && npm install && npm start`.

---

## 22. CI/CD & Deployment

**CI** — `.github/workflows/deploy.yml` runs on every push and PR to `main`:
restore → build `SoberNetwork.Api` (Release) → `dotnet test` both test projects → `npm ci` →
`ng build --configuration production`. (Each test project is run separately; see §15.)

**Deploy** — on push to `main` only, the workflow runs `flyctl deploy --remote-only` to **Fly.io**
(app `sober-network`, region `iad`). Fly builds the Docker image remotely.

**Container** — the multi-stage `Dockerfile` builds the Angular SPA (Node stage), publishes the .NET API
(`sdk:10.0` stage), then copies `dist/sober-network-web/browser` into the runtime image's `wwwroot/`
(`aspnet:10.0`) so ASP.NET Core serves the SPA. The app listens on HTTP **`:8080`** (Fly terminates TLS at the
edge and forwards via `X-Forwarded-*`).

**Ops** — health probe is `GET /health` (anonymous). Production secrets (connection string, `Jwt:Secret`,
`Resend:ApiKey`) are set with `flyctl secrets set` and never committed; non-secret env lives in `fly.toml [env]`.
For local-only Postgres, use `docker-compose.yml` (it is **not** used in production).

---

## 23. Application Bootstrap & Cross-Cutting Middleware (`Program.cs`)

`Program.cs` wires up several behaviors that *enforce* conventions described elsewhere in this document. When
adding endpoints or services, assume the following are already in effect:

- **Global `[Authorize]` is enforced by a fallback authorization policy** (`RequireAuthenticatedUser`). This is
  *why* exposing anything requires `[AllowAnonymous]` **plus** a T11/T12 review comment (see §2). Auth endpoints
  carry that comment (see `AuthController`).
- **MediatR pipeline** — handlers are auto-registered by scanning `SoberNetwork.Core`; a
  `LoggingBehavior<,>` (in `SoberNetwork.Core.Behaviors`) wraps every command/query. Never bypass MediatR (§14).
- **FluentValidation auto-validation** is registered from `SoberNetwork.Core.Validators` — validators run at the
  API boundary, not inside handlers (see §16).
- **Identity policy** — passwords ≥10 chars requiring digit + uppercase + non-alphanumeric; lockout after
  5 failures for 15 min; `RequireConfirmedEmail = true`. JWT validation uses `ClockSkew = TimeSpan.Zero`.
- **Rate limiting** — a named `"auth"` fixed-window limiter (5 requests/min) protects auth endpoints; rejects
  with HTTP 429.
- **Security middleware** — CORS policy `ApiCors` (origins from `Cors:AllowedOrigins`); HSTS in non-Development;
  response headers `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`;
  `UseForwardedHeaders` (for Fly's proxy); `AddProblemDetails()` so errors return `application/problem+json` (§7).
- **Startup side-effects** — `await db.Database.MigrateAsync()` applies pending migrations, then the superuser
  seed runs. Both are wrapped in try/catch so a failure logs an error but does not stop the app from booting.
- **SPA hosting** — `UseDefaultFiles()` + `UseStaticFiles()` serve `wwwroot`; `MapFallbackToFile("index.html")`
  serves the Angular app for non-API routes.
- **Typed options** — `JwtOptions`, `ResendOptions`, `AppOptions` are bound and consumed via `IOptions<T>` (§5).
- **External services** — `Resend` (email) and a named `"Nominatim"` `HttpClient` (geocoding, with a required
  User-Agent) are registered here.
- `public partial class Program { }` exists so `WebApplicationFactory` can boot the app in integration tests
  (see §10 and §15).
