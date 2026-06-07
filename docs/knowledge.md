# Sober Network — Project Knowledge Document

> Living document. Topics marked 🐾 are "dog-ears" — open questions to be discussed and resolved.

> 📖 **Related Guides:** See `.github/copilot-instructions.md` for engineering standards, build commands, and patterns. See `docs/validation-auto-response.md` for FluentValidation auto-response pattern details.

---

## Vision

**Sober Network** is a multi-tenant web platform for Alcoholics Anonymous home groups. Each home group (e.g., *Early Bird Zoom*) gets its own space on the network. Groups self-govern their space while sharing a common infrastructure. New groups can adopt the platform voluntarily.

The platform must honor the **12 Traditions of A.A.** at every design decision.

---

## Core Principles

### Security First
- All users must be authenticated before accessing member content
- Role-Based Access Control (RBAC) governs what each user can see and do
- Public-facing content is strictly limited (meeting times, contact form, no member data)
- Phone lists and member information are never exposed publicly

### AA 12 Traditions — Full Text & Design Implications

These are non-negotiable constraints. Every feature, endpoint, data model, and UX decision must be evaluated against them.

---

**Tradition 1** — *"Our common welfare should come first; personal recovery depends upon A.A. unity."*
- No feature may be designed to serve one member at the expense of the group
- Platform governance must support group unity, not individual prestige or control
- Disputes between members must be resolvable without destroying the group's digital space
- `superadmin` power is a stewardship role, not ownership

**Tradition 2** — *"For our group purpose there is but one ultimate authority — a loving God as He may express Himself in our group conscience. Our leaders are but trusted servants; they do not govern."*
- `group_admin` is a **trusted servant**, not a ruler — the role has limits
- Platform must support group conscience decisions (polls, announcements, voting features)
- No single admin account should be an unrecoverable single point of failure
- Succession planning is a platform feature, not an afterthought

**Tradition 3** — *"The only requirement for A.A. membership is a desire to stop drinking."*
- No demographic, financial, or personal data may be required beyond what's needed for auth
- Sobriety date is **opt-in only** — never required
- Phone number, last name, location — all optional, never exposed publicly
- The join request process must not be a gatekeeping mechanism beyond basic identity

**Tradition 4** — *"Each group should be autonomous except in matters affecting other groups or A.A. as a whole."*
- Groups configure their own content rules, roles, and moderation policies independently
- Platform cannot force one group's settings onto another
- `superadmin` can only intervene if a group's actions affect the platform or other groups
- Multi-group membership must not leak one group's data to another

**Tradition 5** — *"Each group has but one primary purpose — to carry its message to the alcoholic who still suffers."*
- Features must serve recovery, not distract from it
- No advertising, no gamification, no engagement-bait mechanics
- The meeting schedule and newcomer contact features are highest-priority public-facing content
- Every feature should pass the test: "Does this help carry the message?"

**Tradition 6** — *"An A.A. group ought never endorse, finance, or lend the A.A. name to any related facility or outside enterprise, lest problems of money, property, and prestige divert us from our primary purpose."*
- Platform is named **Sober Network**, never "AA [anything]"
- No AA logos, trademarks, or official branding may be used
- No partnerships, sponsors, or affiliate links — ever
- The platform serves AA groups but is not AA itself

**Tradition 7** — *"Every A.A. group ought to be fully self-supporting, declining outside contributions."*
- Each group's costs should be transparent and covered by that group's own voluntary contributions
- No outside donations, VC funding, or sponsorships
- Platform cost dashboard is a planned feature: groups can see their share of running costs
- Payment processing (if ever added) must be voluntary and group-managed, not platform-mandated

**Tradition 8** — *"Alcoholics Anonymous should remain forever non-professional, but our service centers may employ special workers."*
- Platform developers are **volunteers or trusted servants** — not a commercial service provider
- If paid infrastructure is ever used (Fly.io, etc.), it is a service worker cost, not a professional AA service
- No professional counseling, therapy, or clinical features may be offered through the platform

**Tradition 9** — *"A.A., as such, ought never be organized; but we may create service boards or committees directly responsible to those they serve."*
- `superadmin` and platform governance exist to serve groups, not to control them
- Platform structure is flat — no hierarchy between groups
- Groups may organize their own internal structure (officers, committees) through custom roles
- Platform governance docs (how decisions are made) must be public and in the repo

**Tradition 10** — *"Alcoholics Anonymous has no opinion on outside issues; hence the A.A. name ought never be drawn into public controversy."*
- No political content, social cause endorsements, or outside controversy features
- Discussion boards must be scoped to recovery — no off-topic general forums
- Moderation must remove content that draws the group into outside controversy
- Platform itself must never take political or social positions

**Tradition 11** — *"Our public relations policy is based on attraction rather than promotion; we need always maintain personal anonymity at the press, radio, and films level."*
- No public member roster, no public profiles, no searchable member directory
- Last names are never displayed publicly — first name or display name only
- No social media sharing buttons, no "invite friends" viral mechanics
- Public-facing pages are minimal: meeting times, a contact form, nothing more

**Tradition 12** — *"Anonymity is the spiritual foundation of all our traditions, ever reminding us to place principles before personalities."*
- Anonymity is a **first-class architectural constraint**, not a setting
- Display names default to first name only; full names stored but never shown publicly
- No public-facing profile pages
- Member data (phone, email, sobriety date) is accessible only to authenticated group members
- Phone lists are never indexed, cached, or exposed via any public endpoint
- Audit logs record actions, not personal narratives — dignity in every log line

---

## Architecture

### Contextual Admin — Not a Separate Interface

Admin and superadmin affordances surface **in context**, not in a separate control room. This is a deliberate design principle rooted in Tradition 2: the trusted servant is *in the room*, not above it.

**Rules:**
- `group_admin` features (Join Requests, Settings, Chair Schedule, etc.) appear as extra tabs in the Group Hub — invisible to regular members, visible to admins. No separate admin route for group management.
- `superadmin` visiting a group hub sees all group admin tabs plus a quiet **Platform** overlay tab (suspend group, reassign admin). Same page, elevated view.
- `superadmin` visiting a member profile sees an elevated inline action bar (force-confirm email, deactivate, role management). Same profile page, no detour.
- The **only** dedicated `/admin` page is for things with no contextual home: audit log, cost dashboard, health/uptime, platform-wide announcements. It is accessed via a quiet link in the user dropdown — not a nav pill, not a mega-menu.
- The nav pill bar is **identical** for members and superadmins. Power is contextual, not architectural.

### UI Mocks — Design Source of Truth

Self-contained navigable HTML files live in `docs/mocks/`. They are **committed to the repository** and serve as the authoritative design spec for all Angular components and pages.

| File | Purpose |
|------|---------|
| `sn-interactive-mock.html` | **Primary reference.** Navigable: guest/member nav states, Community mega-menu, group hub with all tabs, breadcrumbs, and all major pages. |
| `sn-nav-mock.html` | Site hierarchy tree · nav states · breadcrumb patterns |
| `sn-pages-mock.html` | Guest landing · member community dashboard |

**Workflow:**
1. **Mock is updated first** when a design decision changes — open in browser, iterate in HTML, then implement in Angular.
2. **Components implement to match** — "refer to the mock" is a complete instruction.
3. The mock defines **appearance and UX behaviour** — not implementation. Changes to template structure and CSS are surgical. All existing Angular bindings, `@Input()`/`@Output()` contracts, form controls, service calls, validators, error handling, and business logic are **preserved** unless the mock explicitly shows a behaviour change. Never do a wholesale rewrite of a working component just to match mock styling.
4. **Mock quality = implementation quality.** A vague mock leaves room for interpretation. The more precisely the mock is designed, the less ambiguity there is in code. Invest in the mock first.
5. **When a mock is ambiguous, underspecified, or silent on a detail — stop and ask before implementing.** Don't interpolate. A clarifying question costs one turn; a wrong implementation costs a revert and a rewrite.

**Rebase the mock** periodically to keep it in sync with live components — especially after completing a significant feature. See `docs/mocks/README.md` for the full rebase workflow. **When mock and live code disagree, the live code wins** — rebase updates the mock to match the code, not the reverse (unless intentionally redesigning).

**Token naming:** Mocks use unprefixed tokens (`--bg`, `--cta-bg`). Angular components use `--sn-*` prefix (`--sn-bg`, `--sn-cta-bg`) with literal fallbacks. The translation is mechanical.

**Component annotations in the interactive mock** mark Angular component boundaries as HTML comments (`<!-- ════ <app-hero> ════ -->`) so the mock maps directly to the component tree.

---

### Multi-Tenancy Model
- Each home group = one **tenant**
- Groups get a subdomain: `earlybird.sobernetwork.org`, `groupname.sobernetwork.org`
- Tenant data is logically (and ideally physically) isolated
- A group can join or leave the platform independently

### 🐾 Microservices vs. Modular Monolith
**Open question** — see discussion notes below.

**Arguments for microservices:**
- Services scale independently
- Teams (trusted servants) could own separate domains
- Natural fit for multi-tenancy at scale

**Arguments against (for now):**
- High operational complexity (service mesh, API gateway, CI/CD per service)
- Free/low-cost hosting targets (GitHub Pages, Vercel, Supabase) don't naturally support microservices
- Premature optimization for a platform starting with one home group
- Harder to hand off to non-technical trusted servants

**Recommended approach:** Start with a **Modular Monolith** architected with clean service boundaries. Modules can be extracted into microservices later if scale demands it. This is sometimes called "monolith-first" (per Martin Fowler).

---

## Content Moderation

### Philosophy
Balance AA's spirit of open sharing with protection against disruptive actors ("bombers"). 
Anonymity and member responsibility are both honored.

### Content Areas & Permissions

| Area | Who Can Post | Moderation Model |
|------|-------------|-----------------|
| Announcements | `group_admin` only | N/A |
| Events | `group_admin` only | N/A |
| Documents | `group_admin` only | N/A |
| Phone List | `group_admin` only | N/A |
| Discussion / Sharing Board | Any member | Post-moderation (flag & remove) |
| New member posts (probationary) | New members (<30 days or <10 approved posts) | Pre-moderation queue |

### Probationary Period
- New members' posts go to admin review queue before publishing
- After trust threshold (30 days + 10 approved posts) → posts publish immediately
- Prevents hit-and-run bombers; most never survive the approval step

### Flagging System
- Any member can flag any post as inappropriate
- Flagged content triggers admin notification
- Content remains visible until admin acts (or auto-hides after N flags)

### Admin Actions (escalating severity)
1. **Remove flag** — false alarm, content stays
2. **Hide post** — content removed from view, member notified
3. **Delete post** — permanent removal (soft-delete for audit trail)
4. **Warn member** — formal warning logged
5. **Suspend member** — temporary loss of posting rights
6. **Ban member** — removed from group, account flagged

### Audit Trail
- All moderation actions are logged (who acted, when, what)
- Soft-deleted content retained for `superadmin` review
- Supports accountability without public shaming

### Must-Have (MVP)
- [ ] Group news / announcements
- [ ] Events calendar
- [ ] Document library (group conscience notes, formats, etc.)
- [ ] Phone list (authenticated members only)
- [ ] Email list / group mailing (via groups.io integration or built-in)
- [ ] User authentication + RBAC

### 🐾 Nice-to-Have (discuss priority)
- [ ] Meeting schedule page (public)
- [ ] Daily Readings
- [ ] Bulletin board
- [ ] Speaker meeting audio archive
- [ ] Sobriety chip tracker / anniversary recognition
- [ ] Links to AA literature and resources
- [ ] Multi-language support

### Additional feature ideas

- [ ] Member directory (authenticated only)
  - Define group-scoped visibility and anonymity rules.
  - Add browse/search/filter views for active members.
  - Link member cards to existing profile pages.

- [ ] Meeting signup / chair schedule
  - Let admins publish open service slots for meetings.
  - Allow members to volunteer or request a slot.
  - Show upcoming assignments in the group hub.

- [ ] Service roles tracker
  - Store elected roles with terms and display order.
  - Show current trusted servants on the group hub.
  - Add role history for succession planning.

- [ ] Newcomer welcome resources
  - Add a newcomer-friendly resource page for each group.
  - Let groups list local contacts, readings, and starter info.
  - Surface the resource link prominently in public and member areas.

- [ ] Group conscience / voting tools
  - Create proposals and record votes or consensus outcomes.
  - Support admin review and group discussion before decisions.
  - Keep an audit trail for approved motions and outcomes.

- [ ] Group archives (minutes, history, format docs)
  - Add a private archive library for meeting notes and docs.
  - Support organized folders or tags by document type.
  - Allow admins to upload, update, and retire archived items.

---

## Security & Access Control

### Authentication
- Users must register and be approved (no open self-registration)
- Authentication provider: TBD 🐾 (options: Supabase Auth, Auth0 free tier, GitHub OAuth)

### Roles (proposed)
| Role | Description |
|------|-------------|
| `superadmin` | Platform-level admin (you); can manage all tenants |
| `group_admin` | Trusted servant for a group; manages members and content |
| `member` | Authenticated group member; full read + some write |
| `guest` | Unauthenticated public visitor; sees public content only |

### 🐾 Open Security Questions
- Should members be able to belong to multiple groups?
- How does a new member request access — self-service form, or sponsor-sponsored?
- How do we handle member removal (relapse, misconduct, etc.) gracefully and with dignity?
- Password reset flow — email-based? What if a member's email changes?

---

## Engineering Standards

These standards apply to every feature, PR, and code change on Sober Network. They are standing instructions — not suggestions.

---

### 1. SOLID Principles

**Single Responsibility**
- Every class does one thing. `TokenService` generates tokens. `AuditService` writes audit events. `EmailService` sends email. Never combine concerns.
- Controllers orchestrate only — no business logic in controllers. If a controller method is getting complex, extract a service.

**Open/Closed**
- New behavior is added by implementing new classes against existing interfaces, not by modifying working code.
- Adding a new email provider means implementing `IEmailService` — not editing `EmailService`.

**Liskov Substitution**
- Any implementation of an interface must be fully substitutable. If a test mock can't replace the real service without breaking contracts, the interface is wrong.

**Interface Segregation**
- Interfaces are small and focused. `ITokenService`, `IEmailService`, `IAuditService` — never a God interface that bundles unrelated capabilities.
- If a consumer only needs one method, it shouldn't be forced to depend on ten.

**Dependency Inversion**
- `SoberNetwork.Core` defines interfaces. `SoberNetwork.Infrastructure` implements them. `SoberNetwork.Api` consumes them via DI.
- **Core never imports Infrastructure** — this is enforced by project reference structure.
- Controllers and services depend on interfaces, never on concrete implementations directly.

---

### Architecture: Project Layer Map

| Project | Role | Depends on |
|---------|------|------------|
| `SoberNetwork.Domain` | Entities, Enums — zero local deps | (none) |
| `SoberNetwork.Core` | Commands, Queries, Handlers, Validators, DTOs, Interfaces, Results | Domain |
| `SoberNetwork.Infrastructure` | EF Core, Repositories, Service implementations | Domain, Core |
| `SoberNetwork.Api` | Controllers (IMediator only), DI wiring, Program.cs | Core, Infrastructure, Domain |

**MediatR CQRS pattern (mandatory)**
- All application logic flows through MediatR commands/queries → handlers
- Controllers inject only `IMediator`; all business logic lives in handlers
- Commands return `CommandResult` or `DataResult<T>` using `ResultCode` enum (Ok/BadRequest/Unauthorized/Forbidden/NotFound/Conflict)
- Controllers map `ResultCode` → HTTP status via switch expressions
- `LoggingBehavior` pipeline behavior logs only request type name + elapsed ms — NEVER request/response contents

**Result types** (in `SoberNetwork.Core.Results`)
- `ResultCode` — Ok, BadRequest, Unauthorized, Forbidden, NotFound, Conflict
- `CommandResult(ResultCode, string? Error)` — for commands with no return data
- `DataResult<T>(ResultCode, T? Data, string? Error)` — for data-returning operations

**Auth service pattern**
- `IAuthService` (Core) owns ALL auth workflows: Register, ConfirmEmail, ResendConfirmation, Login, ForgotPassword, ResetPassword, Refresh, Logout
- Auth handlers delegate entirely to `IAuthService`; they contain no auth logic themselves
- Callback URL templates built in controller with `{userId}/{token}` placeholders; `AuthService` substitutes real values

**Build commands** (verified working)
```
dotnet build src\SoberNetwork.Api\SoberNetwork.Api.csproj
cd src\SoberNetwork.Web && npx ng build --configuration=development
dotnet test tests\SoberNetwork.Core.Tests\SoberNetwork.Core.Tests.csproj
dotnet test tests\SoberNetwork.Api.Tests\SoberNetwork.Api.Tests.csproj
```
Note: solution-level `dotnet test` does not work due to `SoberNetwork.Web.esproj` in solution.

---

### 2. API Design Standards

**URL Structure**
- Resources are plural nouns: `GET /api/groups`, `POST /api/groups/{slug}/members`
- Use **slugs** for group identifiers in URLs, never raw database IDs (Tradition 12 — no enumerable IDs for members or groups)
- Nested routes for owned resources: `/api/groups/{slug}/events`, `/api/groups/{slug}/documents`
- Actions that don't fit REST get a verb: `/api/auth/refresh`, `/api/auth/logout`

**HTTP Conventions**
- `GET` — read, never mutates state
- `POST` — create or action
- `PUT` / `PATCH` — full / partial update
- `DELETE` — soft delete only (sets `deleted_at`, never removes rows)

**Authorization Default**
- `[Authorize]` is the default on all controllers. `[AllowAnonymous]` must be explicitly applied AND documented with a comment explaining why it's public.
- Every `[AllowAnonymous]` endpoint is a security decision that must pass Tradition 11/12 review.

**Response Shape**
- Success: return the resource or a plain confirmation message — never raw entity objects
- Error: always return a consistent shape: `{ "message": "..." }` — never stack traces, never Identity error detail in production
- Generic messages on auth endpoints always — no user enumeration

**DTO Boundary**
- Entities (`ApplicationUser`, `Group`, etc.) never leave the API layer. Always map to a DTO/record before returning.
- Input DTOs use `[Required]`, `[MaxLength]`, `[EmailAddress]` etc. — validate at the boundary, not deep in services.

---

### 3. Data Standards

**Every table must have:**
- `created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()`
- `updated_at TIMESTAMPTZ` (updated via EF `SaveChanges` interceptor — planned)
- Soft delete via `deleted_at TIMESTAMPTZ NULL` — never hard delete member content

**Multi-tenancy isolation (Tradition 4)**
- Every table containing member-scoped data must have a `group_id` column with a foreign key to `groups`
- Queries for member data must always include a `group_id` filter — no cross-tenant data leakage ever
- Supabase RLS policies will enforce this at the DB level as a second line of defense

**Naming conventions**
- PostgreSQL tables and columns: `snake_case`
- C# entities and properties: `PascalCase`
- EF Core handles the mapping via `UseSnakeCaseNamingConvention()` or explicit config

**Indexes**
- Always index foreign keys
- Index any column used in a `WHERE` clause in common queries (`group_id`, `user_id`, `created_at`, `slug`)
- Index columns used for lookup uniqueness (`slug` on groups, `token_hash` on refresh tokens)

**Logging / PII (Tradition 12)**
- Logs contain `userId` (GUID) only — never email, display name, sobriety date, phone number, or IP in application logs
- IP address and user agent are stored only in `security_events` for security purposes, never in application/debug logs
- Audit log rows describe *actions*, not *people* — "user X reset their password" not "Scott reset their password"

---

### 4. Tradition Compliance Checklist

Run this checklist before designing or implementing any new feature. If any answer is "yes" or "unclear", stop and resolve it before writing code.

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

---

### 5. Testing Standards

**What to test**
- Handler result-mapping logic in `SoberNetwork.Core` — unit tested with mocked service interfaces (Moq)
- FluentValidation validators — validated with `TestValidate()` / `ShouldHaveValidationErrorFor()`
- All API endpoints — integration tested via `WebApplicationFactory<Program>` with mocked services
- Security-sensitive paths get dedicated tests: auth flows, phone list access, member data isolation, group boundary enforcement

**Test infrastructure**
- `TestWebApplicationFactory` extends `WebApplicationFactory<Program>`
  - Uses `ConfigureAppConfiguration` to inject test JWT/Resend config before app startup
  - Replaces Npgsql with EF Core InMemory database
  - Exposes named `Mock<IGroupService>`, `Mock<IMemberService>`, etc. so tests can `Setup()` them
  - Provides `CreateAuthenticatedClient(userId)` and `CreateSuperAdminClient(userId)` helpers
- `JwtTestHelper.GenerateToken()` creates JWTs with any claims needed for tests
- Solution-level `dotnet test` does not work (SoberNetwork.Web.esproj in solution); run each project separately

**Run tests**
```
dotnet test tests\SoberNetwork.Core.Tests\SoberNetwork.Core.Tests.csproj
dotnet test tests\SoberNetwork.Api.Tests\SoberNetwork.Api.Tests.csproj
```

**Test naming convention**
```
Scenario_condition_expected_outcome (snake_case)
// Examples:
Valid_request_passes
Password_too_short_fails
Outsider_cannot_view_member_list
GetAllGroups_returns_403_for_regular_user
```

**Security test requirements**
Every protected endpoint must have at least:
1. A test asserting unauthenticated requests return `401`
2. A test asserting insufficient role returns `403`
3. A test asserting cross-group access returns `403` or `404` (no data leakage)

**No production secrets in tests**
- Tests use EF Core InMemory database, never the production DB
- JWT secrets and API keys in tests are hardcoded throwaway values only

---

## Technology Stack ✅ DECIDED

### Stack: C# + Angular (developer-familiar, free-hosted)

| Layer | Choice | Notes |
|-------|--------|-------|
| Frontend | **Angular** | SPA, deployed to GitHub Pages or Vercel (free) |
| Backend | **ASP.NET Core Web API** (C#) | REST API, hosted on Render.com or Fly.io (free tier) |
| Database | **Supabase PostgreSQL** | Free tier, 500MB, accessed via EF Core + Npgsql |
| Auth | **ASP.NET Core Identity + JWT** | RBAC baked in, tokens stored client-side |
| File Storage | **Supabase Storage** | Free tier for documents, phone lists |
| Email List | **groups.io** (free) | Handles mailing list; may integrate later |
| CI/CD | **GitHub Actions** | Free for public repos |
| Domain | Custom (~$12/yr, Namecheap/Porkbun) | Subdomains per group |

### ⚠️ Free Hosting Caveat — Backend Cold Starts
Render.com's free tier **spins down** after 15 minutes of inactivity → ~30 second cold start on first visit. 
**Mitigation options:**
- Use **Fly.io** free tier instead (3 shared VMs, stays warmer)
- Add a lightweight ping/keepalive via GitHub Actions cron job
- Accept it for MVP; upgrade to paid (~$7/mo) when groups grow

### Architecture Pattern
**Modular Monolith** (monolith-first, microservice-ready)
- Clean module boundaries: `Auth`, `Groups`, `Events`, `Documents`, `Members`, `Notifications`
- Each module is a self-contained C# project/folder within one solution
- Can be extracted to microservices later if needed

---

## Hosting & Cost

### Target: $0–$20/month total
- **Vercel** (free tier): Frontend hosting
- **Supabase** (free tier): Auth, PostgreSQL DB, file storage
- **GitHub**: Source control, CI/CD via Actions
- **groups.io** (free for small groups): Email list fallback
- **Domain**: ~$12/year (Namecheap or Porkbun)

### 🐾 Cost Scaling Question
As more groups join, Supabase/Vercel free tiers may be exceeded. Should each group:
- (A) Contribute to a shared platform fund (Tradition 7 consideration)?
- (B) Run their own independent instance (fork the repo)?
- (C) Platform absorbs cost up to a threshold, then asks for group contributions?

---

## Domain & Branding

- **Name**: Sober Network
- **🐾 Domain**: Check availability of `sobernetwork.org` / `.com` / `.group` / `.net`
- Logo/branding: TBD — should feel welcoming, not clinical

---

## Open Dog-Ears 🐾 (Discussion Queue)

1. ✅ **Microservices vs. Modular Monolith** — Modular Monolith, monolith-first
2. ✅ **Tech stack choice** — C# (ASP.NET Core) + Angular + Supabase PostgreSQL
3. ✅ **Authentication provider** — ASP.NET Core Identity + JWT
4. ✅ **Multi-group membership** — YES. One user account, multiple group memberships with per-group roles.
5. ✅ **Member onboarding** — Admin-approved. Member submits a join request; `group_admin` approves. No open self-registration into a group.
6. ✅ **Member offboarding** — Soft removal preferred. Admin can warn → suspend → ban. Audit trail retained. Content soft-deleted (hidden, not purged).
7. ✅ **Nice-to-have features** — ALL are MVP. 🐾 Detailed scalability notes per feature to be documented later.
8. ✅ **Domain name** — `sobernetwork.group` (~$15/yr, Namecheap or Porkbun). Subdomains per group: `earlybird.sobernetwork.group`
9. ✅ **Cost scaling model** — Voluntary contributions from each group's 7th Tradition donations. Free to start. Transparent pricing page visible to all group admins. 🐾 Future feature: cost dashboard showing platform running costs, per-group usage, and contribution status.
10. ✅ **Handoff plan** — Use a **GitHub Organization** (`sobernetwork-group` or similar) from day one, not a personal account. Co-admin role for succession. Architecture docs live in the repo. 🐾 Future consideration: define a platform service structure (analogous to AA's GSO) for long-term governance.
11. 🐾 **Set up Playwright MCP server** — TODO (deferred 2026-06-06): configure the Playwright MCP server for browser automation / end-to-end testing of the Angular frontend. Not yet configured.

---

---

## Meeting Finder (Public, Cross-Group)

**Status:** Backend + Angular component complete. ✅

### Design Decisions
- **Public, no login required** (T5 — carry the message; T11/T12 — no member data exposed)
- Only `IsPublic = true` groups appear in the finder (T4 — group autonomy; each group opts in)
- `PublicJoinUrl` is an admin-curated safe URL (e.g. a Zoom link without a password embedded). `ZoomLink` (members-only) is NEVER exposed publicly
- Mailing addresses stored on `ApplicationUser` (opt-in) — used as default search location and for mailing sobriety chips. Unenrolled users enter a starting address or zip code manually
- "Get Directions" CTA links to Google Maps using the meeting's stored address fields. Maps rendered with **Leaflet + OpenStreetMap** (no Google Maps API key required)

### Backend
- `MeetingType` enum: `InPerson=0, Online=1, Hybrid=2`
- `TimeBlock` enum: `Morning, Afternoon, Evening, Night`
- Meeting `Formats` (`text[]`) — 15 canonical values validated by `CreateMeetingRequestValidator`/`UpdateMeetingRequestValidator`: `Discussion`, `Speaker`, `StepStudy`, `TraditionStudy`, `BigBook`, `Literature`, `Topic`, `Beginners`, `Candlelight`, `Meditation`, `BirthdayChip`, `Men`, `Women`, `YoungPeople`, `LGBTQPlus`. Frontend maps these to display labels via `MEETING_FORMAT_LABELS` / `formatMeetingFormat()` in `core/models/group.models.ts`.
- `Meeting` entity fields added: `MeetingType`, `VenueName`, `Street`, `City`, `State`, `PostalCode`, `Country`, `Lat`, `Lon`, `PublicJoinUrl`
- `ApplicationUser` fields added: `MailingStreet`, `MailingCity`, `MailingState`, `MailingPostalCode`, `MailingCountry`, `MailingLatitude`, `MailingLongitude` — all opt-in, never required
- `SearchPublicMeetingsQuery` / `SearchPublicMeetingsQueryHandler` — Haversine distance with bounding-box pre-filter. Earth radius: 3958.8 miles. Default search radius: 25 miles
- `PublicMeetingsController` — `GET /api/meetings` with `[AllowAnonymous]` (T11/T12 review comment present)
- Geocoding: Nominatim (OpenStreetMap) — no API key, free, OSM attribution required

### Angular
- `MeetingFinderComponent` — day chips, time-block and format filters, GPS/geocoding address input, Leaflet map with markers, list view
- "Get Directions" links to `https://www.google.com/maps/dir/?api=1&destination={address}`
- "Join Online" CTA opens `PublicJoinUrl` in new tab (only shown for Online/Hybrid meetings where URL is set)
- Route: `/meetings` (no auth guard)
- Navbar link added

---

## userId Type: Guid

**Status:** Complete refactor ✅. All layers use `Guid` natively.

### Rules
- `ApplicationUser : IdentityUser<Guid>` — Identity tables store uuid columns
- `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- **JWT boundary rule (firm):** JWT claims are `string` by spec. The ONLY conversion points are:
  - `TokenService`: `new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())`
  - Controllers: `private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`
- `UserManager<TUser>` methods (`FindByIdAsync`, etc.) still take `string` — call `.ToString()` at those call sites only
- `IAuditService.LogAsync` takes `Guid? userId` — callers pass Guid directly
- `ResetPasswordRequest.UserId` is `Guid` — ASP.NET JSON body binding handles string→Guid automatically
- `[FromQuery] Guid userId` in `ConfirmEmail` — model binding handles it
- Moq setups for userId args: `It.IsAny<Guid>()` not `It.IsAny<string>()`
- Test URLs with userId in route segments must use valid Guid strings (e.g., `"00000000-0000-0000-0000-000000000001"`) or model binding returns 400 instead of the expected 403
- `TestWebApplicationFactory.CreateAuthenticatedClient(Guid userId = default)` — `default(Guid)` is `Guid.Empty` (valid)

---

## Database Migration Notes

- All old migrations deleted; single `InitialSchema` migration regenerated — all Identity Id columns are `uuid`
- Dev DB must be dropped and recreated when pulling this branch for the first time
- `sober` PostgreSQL user needs `CREATEDB` privilege: `ALTER USER sober CREATEDB;`
  - EF Core's `MigrateAsync` checks for DB existence and attempts `CREATE DATABASE` — requires `CREATEDB` even if DB already exists
- Apply migration: `dotnet ef database update --project src\SoberNetwork.Infrastructure --startup-project src\SoberNetwork.Api`

---

## Superuser Seed

- Seed block in `Program.cs` runs on every startup; skips if superuser already exists
- Reads from user secrets: `Superuser:Email`, `Superuser:Password`, `Superuser:DisplayName`
- Set with: `dotnet user-secrets set "Superuser:Email" "..." --project src\SoberNetwork.Api`
- Seeds roles `superadmin` + `member`; creates user; assigns `superadmin` role; `EmailConfirmed = true`

---

## UI Design System (v1.3 Mockup — Apply Sitewide)

The approved visual design is documented in the v1.3 HTML mockup. All Angular components and pages must follow this system.

### Fonts
- **Inter Tight** — headings, nav, UI labels (weights 400–900, `letter-spacing: -0.02em` to `-0.05em`)
- **Instrument Serif italic** — display accent lines inside headings (e.g. `<em>tradition.</em>`)
- **Inter** — body copy
- Load via Google Fonts: `Inter Tight`, `Instrument Serif`, `Inter`

### Color Palette
```
--bg:        #f7f5f2   (page background — warm off-white)
--bg-soft:   #f0ede8   (alternate section background)
--text:      #111110   (primary text)
--text-mid:  #3d3d3a   (secondary text)
--text-muted:#7a7a75   (captions, labels)
--border:    rgba(17,17,16,0.08)
--cta-bg:    #1a1a18   (dark CTA/button background)
```
> **Angular app:** these are exposed `--sn-*` prefixed (`--sn-bg`, `--sn-cta-bg`, … in `styles.scss :root`). Components must use `var(--sn-cta-bg, #1a1a18)` with a literal fallback; the unprefixed names above are the mockup's. `var(--cta-bg)` resolves to nothing and silently breaks styling.

### Hero Gradient
```css
background-image:
  linear-gradient(135deg,
    rgba(212,232,245,0.88) 0%,
    rgba(232,240,248,0.85) 20%,
    rgba(247,245,240,0.82) 55%,
    rgba(245,232,216,0.85) 80%,
    rgba(240,223,200,0.88) 100%),
  url('<photo>');
background-size: cover;
```
Used on `#hero` and `#final-cta`. The photo sits behind the gradient at ~12% opacity.

### Feature / Icon Tints (8 cheerful tint colors)
```
violet: hsla(260,70%,65%,0.12)   rose:   hsla(345,75%,65%,0.12)
sky:    hsla(205,85%,60%,0.12)   amber:  hsla(38,90%,58%,0.12)
green:  hsla(148,58%,52%,0.12)   teal:   hsla(183,62%,52%,0.12)
indigo: hsla(230,70%,62%,0.12)   coral:  hsla(18,82%,62%,0.12)
```
Applied to: feature card icon backgrounds, About section badge pills, marquee icon bubbles.

### Nav Logo
CSS conic-gradient rainbow ring around the inner SVG:
```css
background: conic-gradient(from 0deg, #f06292, #ff9800, #ffeb3b, #4caf50, #2196f3, #9c27b0, #f06292);
border-radius: 50%;
padding: 2.5px;
```
Inner SVG has `background: white; border-radius: 50%`.

### Nav Bar
- White card pill container with inset box-shadow, `border-radius: 100px`
- Active nav item: solid black pill (`background: var(--text); color: white`)
- Nav gradient accent line: `nav::after` with `linear-gradient(90deg, sky→violet→amber→coral)` at `bottom: -1px`
- Sign In: ghost link; Join CTA: dark gradient pill with frosted arrow circle

### Section Cadence (alternating backgrounds)
```
#hero        → hero-grad + photo bg
marquee      → white
#about       → white
#features    → --bg (off-white)
#how-it-works→ white
#newcomer    → --cta-bg (dark)
#principles  → white
#roadmap     → --bg (off-white)
#traditions  → white
#final-cta   → hero-grad
```

### Image Treatment
- **Real photos** (Vecteezy free): used as hero background at ~15% opacity behind gradient
- **Vector art** (Vecteezy free): `mix-blend-mode: multiply` + `filter: brightness(1.35) contrast(1.05)` removes white/off-white backgrounds cleanly
- All section images hosted via `static.vecteezy.com` CDN using `non_2x` preview URLs

### Mockup File Location
```
C:\Users\scott\.copilot\session-state\dc627e7b-f3a3-4928-8621-16da3c6acb7f\files\
  sober-network-mockup-v1.2.html   ← last stable rollback before images
  sober-network-mockup-v1.3.html   ← current approved design with images ✅
```

### Landing Page — Angular Implementation Notes

- **Hero height**: `padding: 110px 32px 90px` with `justify-content: flex-start` — no `min-height`. Height is content-driven, matching ~70% viewport appearance of the mock.
- **Back-to-top buttons**: Each named section (`#about`, `#features`, `#how-it-works`, `#principles`, `#traditions`) has a `.back-to-top` anchor inside `.section-wrap` (which is `position: relative`). Button is `position: absolute; top: 32px; right: 32px`.
- **Section IDs used by scroll-spy**: `about`, `features`, `how-it-works`, `roadmap`, `traditions` (in `navbar.component.ts` `sectionIds` array).

### Navbar — Angular Implementation Notes

- **Position**: `position: fixed; top: 0; left: 0; right: 0; z-index: 200`. App content has `padding-top: 64px` to compensate (`app.scss`).
- **Layout**: CSS Grid `grid-template-columns: 1fr auto 1fr` — logo left, pills center, actions right.
- **Scroll-spy**: `IntersectionObserver` in `navbar.component.ts`, wrapped in `NgZone.run()` + `cdr.markForCheck()`. Re-initialized on `NavigationEnd` to `/`.
- **Guest state**: single dark pill **"Sign In"** button → opens `LoginModalComponent` via `MatDialog`. No separate "Join Your Group" link — registration is discovered via "Don't have an account? Create one" inside the login modal.
- **Logged-in state**: pills link to app routes. User pill (`display-name + initial`) opens `MatMenu`.

### Logo

- Rainbow conic-gradient ring with white inner circle
- Inner SVG: **upward-pointing equilateral triangle** (AA-style), stroke only, no fill
  - `<polygon points="15,3 25,21 5,21" fill="none" stroke="#1a1a18" stroke-width="2" stroke-linejoin="round"/>`
  - viewBox `0 0 30 30`; inscribed in the circle
- Applied consistently to: navbar, login modal, register modal

### Form Modals — BaseFormModalComponent (composition pattern)

- All form modals **wrap** `<app-base-form-modal>` (composition, **not** inheritance) and supply their own form via `ng-content`. See `.github/copilot-instructions.md` §19 for the full copy-paste recipe.
- `BaseFormModalComponent` (`src/app/shared/components/base-form-modal/`) renders the white card (`.lm-wrap`), close button, rainbow-ring icon, title, and subtitle. The card caps at `90vh` and scrolls its body (`.lm-body`) while the header/close stay fixed, so tall forms keep their buttons reachable.
- Modals using this pattern: `LoginModalComponent`, `RegisterModalComponent` (`shared/components`), `GroupFormModalComponent`, `MeetingFormModalComponent` (`features/groups`).
- **Always** open with `MatDialog` `panelClass: ['sn-modal-panel', 'sn-<modal>-panel']` — `sn-modal-panel` is the generic base (in `styles.scss`) that makes the Material surface transparent so the component's `.lm-wrap` owns the styling; the second class is a **per-modal decorator hook**. Each modal has one (`sn-login-panel`, `sn-register-panel`, `sn-group-panel`, `sn-meeting-panel`) and sets its own pane **width** there — the `open()` config only sets `maxWidth: '100vw'`. The older `sn-form-modal` panel class has been removed.
- Field styling uses the shared `.lm-*` classes (`.lm-form`, `.lm-field`, `.lm-input`, `.lm-field-error`, `.form-actions`, …) — never Material form fields.

### Login Modal

- Component: `src/app/shared/components/login-modal/login-modal.component.ts`
- Opened via `MatDialog` from `NavbarComponent.openSignIn()` with `panelClass: ['sn-modal-panel', 'sn-login-panel']` (generic base + login decorator)
- Global panel override in `styles.scss`: `.sn-modal-panel .mat-mdc-dialog-surface` — removes Material padding, applies `border-radius: 20px` and custom shadow. Shared by **all** form modals; `.sn-login-panel` is the login decorator and sets the login pane width (`440px`).
- On success: closes modal, navigates to `/dashboard`
- "Forgot password?" → closes modal, navigates to `/auth/forgot-password`
- "Create one" → dynamically imports `RegisterModalComponent` and opens it (no navigation)
- The existing `/auth/login` page is still reachable (used by `authGuard` redirects for protected routes)

### Register Modal

- Component: `src/app/shared/components/register-modal/register-modal.component.ts`
- Same `lm-*` CSS design as login modal (rainbow ring, Inter Tight, dark pill submit)
- Fields: Display Name, Email, Password, Confirm Password
- Success state: green check icon + email confirmation message
- "Sign in" → dynamically imports `LoginModalComponent` and opens it
- Both modals use **async dynamic imports** to cross-open each other, avoiding circular TypeScript deps
- `/auth/register` page route kept alive for `login.component` and `group-public` fallback links
- Landing page hero, final-CTA, and "Get started" buttons all open `RegisterModalComponent`

### Auth Redirect Behavior

- `AuthService.logout()` navigates to `/` (landing page), NOT `/auth/login`
- On page refresh with an expired refresh token: `APP_INITIALIZER` → `tryRestoreSession()` → 401 → `errorInterceptor` → `auth.logout()` → `/` (no redirect to login page)
- Protected routes (`/dashboard`, `/groups`, etc.) still redirect to `/auth/login` via `authGuard` when unauthenticated

---

## Platform Stats API

**Status:** Complete ✅

### Design Decisions
- **Public, no login required** (T11/T12 reviewed: aggregate integer counts only, no member data)
- Three counts returned: distinct active members, active groups, active meetings
- Member count = distinct `UserId` in `GroupMemberships` where `Status=Active` and `DeletedAt=null`
- Group count = `Groups` where `DeletedAt=null`
- Meeting count = `Meetings` where both meeting and its group have `DeletedAt=null`

### Backend
- `IStatsService` / `StatsService` (Infrastructure) — three `AsNoTracking` COUNT queries
- `GetPlatformStatsQuery` → `GetPlatformStatsQueryHandler` → `IStatsService`
- `PlatformStatsResponse(MemberCount, GroupCount, MeetingCount)` record DTO
- `StatsController` — `GET /api/stats` with `[AllowAnonymous]` (T11/T12 review comment present)

### Angular
- `StatsService` at `src/app/core/services/stats.service.ts`
- `LandingComponent.ngOnInit()` loads stats; errors silently fall back to zero
- `stat-plus` (`+`) shown only when count > 0

---

## Security & Performance Audit (2026-06-06)

**Status:** Backlog cleared ✅ (commits `4023c76`, `ed4db4c`)

An audit of the codebase against the standing instructions produced a prioritized
backlog. All High/Medium/Low findings are resolved.

### Durable facts learned
- **Global auth fallback policy** — `Program.cs` registers
  `AddAuthorizationBuilder().SetFallbackPolicy(RequireAuthenticatedUser())`. Authentication
  is enforced by default; a missing `[Authorize]` is **clarity/defense-in-depth only**, not a
  vulnerability. Only `[AllowAnonymous]` endpoints are public.
- **Pagination binding** — bind paging DTOs with `[FromQuery] PaginationQuery`, **never**
  `[AsParameters]` (minimal-API only; in MVC a complex param defaults to `[FromBody]` and a
  bodyless GET returns 415). FluentValidation auto-validation then returns 400 for invalid paging.
  See `PaginationQuery` + `PaginationQueryValidator` (Core) and `PaginationValidationTests`.
- **`[ApiController]` auto-validates** DataAnnotations → 400 ProblemDetails before the action,
  so manual `if (!ModelState.IsValid)` checks are redundant.

### Fixes applied
- **N+1 removed** in `GroupService.GetUserGroupsAsync` / `MemberService.GetAllMembersAsync`
  (single grouped count via `GroupBy`/`ToDictionaryAsync`).
- **DB indexes** (migration `AddIndexesForQueryColumns`): `group_memberships(created_at;
  group_id,status,deleted_at)`, `meetings(created_at)`, `refresh_tokens(created_at)`,
  `groups(created_at)`. Applied on startup via `MigrateAsync()`.
- **Log hygiene (T12)** — `ClientLogController` scrubs email/phone before logging; removed
  email from superuser-seed logs; dropped meeting name from `MeetingService` create log.
- **Explicit `[Authorize]`** added to Groups/Meetings/Members controllers.
- **Tests** — `MeetingSecurityTests` (401/403/cross-group for 5 endpoints + client-log 401);
  `PaginationValidationTests`; security tests renamed to `Method_Scenario_Result`.
- **Documented exceptions** in copilot-instructions.md: read-query `Result<T>` convention; and
  self-scoped/global-superadmin endpoints don't need cross-group 403 tests.

---

## Group Hub Design System (v1.0 — Apply to All Authenticated Pages)

**Status:** Live ✅ (group hub is the reference implementation — extend this theme to all inner-app pages)

The group hub established a distinct "app interior" design language that differs from the landing page. All authenticated pages (`/dashboard`, `/profile`, `/groups/:slug`, `/admin`) should use this system.

### Page Shell

```scss
// Outer shell — max-width container
.hub-shell {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px 80px;
}

// Primary page card
.page {
  background: rgba(255,255,255,.9);
  border: 1px solid rgba(17,17,16,.09);
  border-radius: 28px;
  overflow: hidden;
  box-shadow: 0 16px 52px rgba(17,17,16,.07);
  margin-top: 12px;
}
```

### Tab Rail

```scss
.tab-rail {
  padding: 16px 20px 0;
  border-bottom: 1px solid rgba(17,17,16,.09);
  background: rgba(247,245,242,.8);
  display: flex; flex-wrap: wrap; gap: 4px;
}
.tab-btn {
  border-radius: 14px 14px 0 0;
  font-family: 'Inter Tight', sans-serif;
  font-weight: 800;
  font-size: .88rem;
  &.active { background: white; border-color: rgba(17,17,16,.09); border-bottom-color: white; }
}
```

### Content Cards

```scss
// Standard card
.card {
  background: white;
  border: 1px solid rgba(17,17,16,.09);
  border-radius: 18px;
  padding: 18px;
  box-shadow: 0 6px 20px rgba(17,17,16,.04);
}

// Warm neutral card (descriptive/about content)
.about-card {
  background: linear-gradient(135deg, rgba(248,246,241,.9), rgba(240,237,232,.95));
}

// Entity rows (person/role list items)
.entity-row {
  background: var(--sn-bg-soft, #f0ede8);
  border: 1px solid rgba(17,17,16,.06);
  border-radius: 14px;
  padding: 14px;
}
```

### Section Headings

```scss
.section-heading {
  font-family: 'Inter Tight', sans-serif;
  font-size: .7rem;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: .09em;
  color: var(--sn-text-muted, #7a7a75);
}
```

### Accent Colors (Semantic Card Gradients)

Each card type gets a tinted gradient drawn from the landing page's 8-tint palette:

| Purpose | Gradient |
|---------|---------|
| Next meeting / time / sky | `rgba(14,165,233,.1)` → `rgba(42,148,89,.1)` |
| Membership / role / violet-indigo | `rgba(138,79,163,.07)` → `rgba(79,70,229,.06)` |
| Admin / warning tab badge | `rgba(138,79,163,.12)` bg, `#8a4fa3` text |
| Destructive / leave | `rgba(220,38,38,.1)` bg, `#b91c1c` text |

### Avatar Palette (7 deterministic colors)

Assign by hashing `userId` → `index % 7`:

```scss
.av-violet { background: rgba(138,79,163,.22);  color: #6d3e86; }
.av-sky    { background: rgba(14,165,233,.18);  color: #0369a1; }
.av-green  { background: rgba(42,148,89,.18);   color: #166534; }
.av-rose   { background: rgba(244,63,94,.16);   color: #be185d; }
.av-amber  { background: rgba(217,119,6,.18);   color: #92400e; }
.av-teal   { background: rgba(15,118,110,.18);  color: #0f766e; }
.av-coral  { background: rgba(249,115,22,.16);  color: #c2410c; }
```

Border-radius: `12px`. Size: `42×42px`. Single initial, `font-weight: 900`.

### Chip Palette (9 colors + muted)

```scss
.chip-violet { background: rgba(138,79,163,.12); color: #8a4fa3; }
.chip-sky    { background: rgba(14,165,233,.12);  color: #0ea5e9; }
.chip-green  { background: rgba(42,148,89,.12);   color: #2a9459; }
.chip-amber  { background: rgba(217,119,6,.12);   color: #d97706; }
.chip-rose   { background: rgba(217,53,154,.12);  color: #d9359a; }
.chip-teal   { background: rgba(15,118,110,.12);  color: #0f766e; }
.chip-indigo { background: rgba(79,70,229,.12);   color: #4f46e5; }
.chip-coral  { background: rgba(234,94,61,.12);   color: #ea5e3d; }
.chip-muted  { background: rgba(17,17,16,.06);    color: #7a7a75; }
```

Base chip: `padding: 5px 10px; border-radius: 999px; font-size: .78rem; font-weight: 800; font-family: 'Inter Tight'`. Small variant (`.chip-small`): `font-size: .68rem; padding: 2px 8px`.

### Hero Serif Accent (Group/Page Suffix)

The `titleSuffix` in `GroupHeroComponent` (and any hero using `<span class="serif-accent">`) renders on its own line, right-aligned and nudged right:

```scss
.serif-accent {
  font-family: 'Instrument Serif', serif;
  font-style: italic;
  font-weight: 700;
  display: block;
  text-align: right;
  transform: translateX(3rem);
}
```

This creates a visual "stagger" where the bold group name anchors left and the italic suffix floats right — intentional and designed.

### Reference Implementation

- **Hub shell + tabs**: `src/app/features/groups/group-hub/group-hub.component.scss`
- **Overview cards, avatars, chips**: `src/app/features/groups/group-hub/tabs/overview-tab/overview-tab.component.scss`
- **Hero serif accent**: `src/app/features/groups/group-hero/group-hero.component.scss`

---

*Last updated: 2026-06-07*
