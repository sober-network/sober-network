# Sober Network - Copilot Instructions

Repository guidance for future Copilot sessions. Keep changes aligned with `docs/knowledge.md` and `docs/mocks/sn-interactive-mock.html`.

## Build, test, and lint commands

### Backend (.NET 10)

```powershell
dotnet restore src\SoberNetwork.Api\SoberNetwork.Api.csproj
dotnet build src\SoberNetwork.Api\SoberNetwork.Api.csproj
dotnet build src\SoberNetwork.Api\SoberNetwork.Api.csproj --no-restore --configuration Release
dotnet test tests\SoberNetwork.Core.Tests\SoberNetwork.Core.Tests.csproj
dotnet test tests\SoberNetwork.Api.Tests\SoberNetwork.Api.Tests.csproj
```

Run a single backend test:

```powershell
dotnet test tests\SoberNetwork.Core.Tests\SoberNetwork.Core.Tests.csproj --filter "FullyQualifiedName~GetPhoneListQueryHandlerTests"
dotnet test tests\SoberNetwork.Api.Tests\SoberNetwork.Api.Tests.csproj --filter "FullyQualifiedName~MeetingSecurityTests"
```

> Use the project-level test commands above instead of solution-level `dotnet test`; the Angular `.esproj` is not testable by `dotnet test`.
> CI parity: `.github\workflows\deploy.yml` runs backend build/tests in Release and the Angular production build, so use those flags when validating CI-sensitive changes.

### Frontend (Angular 21)

Package scripts in `src\SoberNetwork.Web\package.json` mirror these commands (`start`, `build`, `test`, `e2e`, `e2e:ui`, `e2e:debug`, `e2e:report`).

```powershell
cd src\SoberNetwork.Web
npm ci
npm start
npm run build
npm run test
npm run e2e
npx ng build --configuration=development
npx ng build --configuration=production
npx ng test --watch=false
```

Run a single frontend unit test file:

```powershell
cd src\SoberNetwork.Web
npx ng test --watch=false --include="src/app/path/to/component.spec.ts"
```

### E2E (Playwright)

```powershell
cd src\SoberNetwork.Web
npm run e2e
npm run e2e:ui
npm run e2e:debug
npm run e2e:report
```

Run a single E2E file or test:

```powershell
cd src\SoberNetwork.Web
npx playwright test e2e\landing.spec.ts
npx playwright test -g "Auth Flow"
```

Playwright targets `http://localhost:4200` and starts `npm start` automatically if the dev server is not already running.

### Linting / formatting

- No dedicated lint script is wired in `package.json` or CI.
- Optional formatting:

```powershell
cd src\SoberNetwork.Web
npx prettier --write "src/**/*.{ts,html,scss}"
```

## High-level architecture

- Repository shape: `src\SoberNetwork.{Domain,Core,Infrastructure,Api,Web}` with tests split into `tests\SoberNetwork.Core.Tests` and `tests\SoberNetwork.Api.Tests`.
- Modular monolith with four projects:
  - `src\SoberNetwork.Domain` - entities and enums
  - `src\SoberNetwork.Core` - commands, queries, handlers, DTOs, validators, result types, interfaces
  - `src\SoberNetwork.Infrastructure` - EF Core data and service implementations
  - `src\SoberNetwork.Api` - ASP.NET Core host and controllers
- Primary backend flow: Controller -> MediatR command/query -> Core handler -> service interface -> Infrastructure service -> `AppDbContext`.
- Auth is private by default because `Program.cs` sets a fallback authorization policy; endpoints are public only when intentionally marked `[AllowAnonymous]`.
- The API serves the SPA and uses `MapFallbackToFile("index.html")` for client-side routing.
- In Development, the API's SPA proxy launches Angular with `npm start`.
- Frontend routes are split by audience: public (`/`, `/group/:slug`, `/meetings`), auth (`/auth/...`), member (`/dashboard`, `/groups/:slug`, profile pages), and superadmin (`/admin`).
- Admin and superadmin capabilities are contextual, not a separate interface. Group Hub and member profile pages surface elevated actions inline; the dedicated `/admin` page is only for platform-wide tools.
- Group slugs are the public identifier in URLs. Do not expose raw database IDs.
- The nav pill bar is identical for members and superadmins.
- `docs/mocks/sn-interactive-mock.html` is the design spec for Angular UI, and its HTML comment markers map back to component boundaries.
- The group hub is the reference implementation for authenticated "app interior" pages.
- CI on `.github\workflows\deploy.yml` builds/tests the backend in Release and builds Angular in production on pushes or PRs to `main`.
- `Program.cs` centralizes fallback auth, MediatR logging, FluentValidation, rate limiting, security headers/problem details, startup migrations/seeding, and SPA hosting; changes to those concerns usually belong there.

## Key conventions

### Security, anonymity, and public endpoints

- Treat the AA Traditions constraints in `docs/knowledge.md` as architecture-level requirements, especially anonymity-related T11/T12 and autonomy T4.
- `[Authorize]` is effectively the default because of the fallback policy.
- When adding `[AllowAnonymous]`, document why public access is intentional and how member or PII data is protected.
- `LoggingBehavior` logs only the request type and elapsed milliseconds; never request or response payloads.

### API + MediatR result pattern

- Keep controllers thin; business rules belong in handlers and services.
- Handlers commonly return `DataResult<T>` or `CommandResult` and map service error text to `ResultCode` with string checks such as `"permission"`, `"already"`, `"not a member"`, `"only admin"`, or `"Incorrect"`.
- Controllers map `ResultCode` to HTTP responses with feature-specific switch expressions.
- Some read paths intentionally return DTO/null directly instead of `DataResult<T>`; follow the local pattern.
- Entities never leave the API layer; map them to DTOs or records before returning.

### Validation and DTO boundaries

- FluentValidation auto-validation is enabled in `Program.cs` with `AddFluentValidationAutoValidation` and `AddValidatorsFromAssemblyContaining<...>()`.
- Keep request DTOs in `Core\DTOs\{Feature}` and validators in `Core\Validators\{Feature}` using `{RequestName}Validator`.
- For MVC paging, bind `PaginationQuery` with `[FromQuery]`; do not use `[AsParameters]`.
- Avoid manual `ModelState` checks for failures already handled by FluentValidation.

### Data and tenancy boundaries

- Use group slugs in URLs; never expose raw database IDs.
- `AppDbContext` uses Identity with `Guid` keys and snake_case table names.
- Member-scoped data access must stay filtered by `GroupId`; preserve group isolation in all new queries.
- Soft delete (`DeletedAt` / `deleted_at`) is the established pattern for group, member, and meeting records.
- Meeting `Formats` and `DaysOfWeek` are PostgreSQL arrays (`text[]`, `integer[]`) with SQL defaults in EF configuration.
- All schema changes go through EF migrations only. Use `dotnet ef migrations add ... --project src\SoberNetwork.Infrastructure --startup-project src\SoberNetwork.Api`, `dotnet ef database update`, and `dotnet ef migrations remove` before pushing if a migration must be rolled back. Pending migrations are applied on startup via `MigrateAsync()`.

### Frontend implementation and design system

- UI is mock-first: update `docs/mocks/sn-interactive-mock.html` first, then implement to match.
- Keep UI edits surgical: preserve existing bindings, form controls, service calls, validators, and business logic unless the behavior change is explicit.
- Use the mock's HTML comments when tracing or rebasing component structure.
- Mock tokens use unprefixed names; Angular styles use `--sn-*` tokens with literal fallbacks such as `var(--sn-cta-bg, #1a1a18)`.
- The v1.3 design system in `docs/knowledge.md` applies sitewide: Inter Tight for headings/nav/UI labels, Instrument Serif italic accents, Inter for body copy, the warm off-white palette, and the dark CTA color.
- Form modals are composition-based, not inheritance-based: `BaseFormModalComponent` wraps the shell, child modals project content through `ng-content`, and shared `.lm-*` classes keep the styling consistent. Do not rebuild them as ad hoc Material forms.
- Angular dialogs use `MatDialog` with `panelClass: ['sn-modal-panel', 'sn-<modal>-panel']`; auth modals rely on dynamic imports to avoid circular dependencies.
- Reactive forms should stay typed (`FormControl<T>`, `nonNullable: true`), and use `form.get(...)` when that keeps template bindings type-safe.
- Group Hub uses a single `groups/:slug` route; do not reintroduce separate route-per-tab patterns.

### Test infrastructure patterns

- API integration tests use `tests\SoberNetwork.Api.Tests\Infrastructure\TestWebApplicationFactory` with exposed service mocks such as `GroupService`, `MemberService`, and `AuthService`.
- Use `CreateAuthenticatedClient(...)` and `CreateSuperAdminClient(...)` instead of hand-rolled auth setup.
- Use `JwtTestHelper.GenerateToken(...)` for test JWT creation when direct token generation is needed.

### Building new features

- When adding a feature, start with database entities (Domain), then create handlers/DTOs/validators (Core), implement services (Infrastructure), wire the controller (Api), and finally add Angular components (Web).
- Example reference: **News & Announcements** — A Facebook-style post feed with recursive replies (unlimited depth). See:
  - Entities: `SoberNetwork.Domain/Entities/Post.cs`, `PostComment.cs`
  - Backend: `src/SoberNetwork.Core/Handlers/News/` (9 CQRS handlers)
  - Controller: `src/SoberNetwork.Api/Controllers/NewsController.cs`
  - Frontend: `src/SoberNetwork.Web/src/app/features/news/` (recursive `CommentSectionComponent`, media support, emoji picker)
- For recursive/nested entities (comments within comments, replies to replies):
  - Store parent reference as a nullable `Guid` (e.g., `ParentCommentId`) — do not use composite tree structures
  - Return the full tree in a single query when possible (EF projections handle recursion well)
  - In Angular, use recursive components (`*ngIf="item.children?.length"`) to render unbounded nesting
  - Lazy-load deeply nested replies if performance becomes an issue

### Local development setup

- `docker compose up -d` starts the local PostgreSQL container.
- API user secrets hold `Jwt:Secret`, `Resend:ApiKey`, and the `Superuser:*` seed values; do not commit them.
