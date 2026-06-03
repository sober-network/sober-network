# Sober Network — Copilot Standing Instructions

These instructions apply to every task, feature, PR, and code change on this repository.
They are derived from `docs/knowledge.md` and are non-negotiable. Always apply them.

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
- Input DTOs validate at the boundary: `[Required]`, `[MaxLength]`, `[EmailAddress]`, etc.

---

## 3. SOLID Principles

**Single Responsibility** — Every class does one thing. No business logic in controllers; controllers orchestrate only.

**Open/Closed** — New behavior via new classes implementing existing interfaces, not by modifying working code.

**Liskov Substitution** — Any implementation of an interface must be fully substitutable.

**Interface Segregation** — Interfaces are small and focused (`ITokenService`, `IEmailService`, `IAuditService`). No God interfaces.

**Dependency Inversion:**
- `SoberNetwork.Core` defines interfaces.
- `SoberNetwork.Infrastructure` implements them.
- `SoberNetwork.Api` consumes them via DI.
- **Core never imports Infrastructure** — enforced by project reference structure.
- Controllers and services depend on interfaces, never on concrete implementations directly.

---

## 4. API Design Standards

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
- Success: return the resource or a plain confirmation — never raw entity objects
- Error: `{ "message": "..." }` — never stack traces, never Identity error detail in production

---

## 5. Data Standards

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

## 6. Logging / PII (T12)

- Logs contain `userId` (GUID) **only** — never email, display name, sobriety date, phone number, or IP in application logs
- IP and user agent are stored only in `security_events` for security purposes
- `ClientLogData = Record<string, string>` — all log metadata values must be strings; convert with `String()` at the call site
- Audit log rows describe *actions*, not *people*

---

## 7. Testing Standards

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

## 8. Architecture Reminders

- **Modular Monolith** — clean module boundaries: `Auth`, `Groups`, `Events`, `Documents`, `Members`, `Notifications`
- `IsPublic` on `Group` controls directory discoverability only — NOT direct-link access. Private groups can still share join links.
- `group_admin` is a trusted servant role — succession planning is a platform feature, not an afterthought.

---

## 9. Build & Validation Commands

```powershell
# Backend
dotnet build src\SoberNetwork.Api\SoberNetwork.Api.csproj

# Frontend
cd src\SoberNetwork.Web && npx ng build --configuration=development
```

Always run both after making changes.
