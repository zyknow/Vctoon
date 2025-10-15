# Copilot Instructions for this Repository

Chat
Use Zh-CN language for responses
When building, only run `dotnet build` for the csproj files you changed; do not build the whole solution by default.

Project stack

- .NET 9, C# latest
- ABP Framework 9.3.x (monolith)
- UI: Razor Pages (prefer Razor Pages over MVC or Blazor)
- Data: EF Core 9 via ABP repositories
- Real-time: SignalR

Solution layout (projects)

- Vctoon.Domain: Domain entities, aggregates, domain services, constants, error codes
- Vctoon.Domain.Shared: Cross-layer contracts (enums, permissions, error codes, localization resources)
- Vctoon.Application.Contracts: DTOs, Application service interfaces, permission names
- Vctoon.Application: Application services, AutoMapper profiles, business orchestration
- Vctoon.EntityFrameworkCore: DbContext, configs, EF mappings, migrations
- Vctoon.HttpApi: API controllers (expose Application services)
- Vctoon.Web: Razor Pages UI, SignalR hubs, page models, client assets
- Test projects: unit/integration tests per layer

General conventions

- Prefer asynchronous APIs; suffix async methods with "Async"
- Enable nullable; avoid nulls in DTOs where possible
- Use file-scoped namespaces, expression-bodied members when clear
- Inject dependencies via constructor; do not use service locator
- Use records for immutable DTOs; classes for entities and mutable models
- Use GUID primary keys unless the domain dictates otherwise
- Use ILogger<T> for logging; favor structured logs
- Always check permissions in UI and endpoints

ABP-specific conventions

- Use IRepository<TEntity, TKey> for data access inside Application/Domain services
- Do not inject DbContext directly outside EF layer; prefer repositories
- Use Unit of Work (ambient by default) for transactional operations
- Throw BusinessException with error codes for business rule violations
- Define permissions in VctoonPermissions (Domain.Shared) and check with [Authorize] or IAuthorizationService
- Use IObjectMapper (AutoMapper) for Entity <-> DTO mapping; keep mapping profiles in Application
- Localize strings via L["Key"]; add to localization resources under Domain.Shared
- Use AbpValidation (data annotations) on DTOs; validate inputs on app services
- Use AbpBackgroundJobs only if needed; otherwise keep synchronous

Razor Pages (preferred UI)

- Page models should inherit from AbpPageModel (or project base page model if present)
- Use PageModel handlers (OnGetAsync/OnPostAsync) and strongly-typed PageModel properties
- Authorize pages with [Authorize(Permissions.X)] and use Policy-based checks in code when needed
- Use Tag Helpers and abp dynamic script/style tags when applicable
- Keep JS minimal; prefer server-side operations unless interactivity demands otherwise

HTTP API

- Controllers in Vctoon.HttpApi inherit from AbpControllerBase or project base controller
- Expose Application services; do not place business logic in controllers
- Route convention: [Route("api/[controller]")] unless project-level base path exists
- Return DTOs (PagedResultDto<T>, ListResultDto<T>, etc.)

Entity Framework Core

- Keep DbContext and entity configurations in Vctoon.EntityFrameworkCore
- Use Fluent API in IEntityTypeConfiguration<T> for mapping; avoid data annotations for persistence details
- Migrations live under EntityFrameworkCore/Migrations; name with meaningful prefixes
- Avoid lazy loading; include explicitly via repository queryable or specification

Domain layer

- Keep domain rules near aggregates; enforce invariants in constructors/methods
- No infrastructure dependencies in Domain; only abstractions
- Put constants into VctoonConsts and error codes into <Feature>DomainErrorCodes

SignalR

- Hubs live in Vctoon.Web; use strongly-typed hub methods
- Group/connection management through IHubContext where needed
- Authorize with [Authorize] and limit payload size as needed

Caching

- Prefer IDistributedCache<T> for cross-instance caches
- Use cache keys with clear prefixes and include tenant/user where applicable

Testing

- Use ABP TestBase and xUnit
- Arrange-Act-Assert; prefer explicit dataset creation over shared state
- For EF tests, use the EF test project and transactional tests when possible

Error handling and responses

- Map business errors to ProblemDetails automatically via ABP
- Never leak internal exceptions; wrap with BusinessException and error codes

Versioning and packages

- Target .NET 9; keep ABP at 9.3.x compatible packages
- Prefer Microsoft and ABP packages; avoid niche dependencies without justification

Security

- Enforce authorization at app service or controller level by default
- Validate all inputs; guard against over-posting by using explicit DTOs
- Use anti-forgery on Razor Pages posts (enabled by default)

Configuration

- Use appsettings.{Environment}.json and user secrets for secrets
- Keep connection strings and credentials out of source control

Naming

- DTOs: <Entity>Dto, CreateUpdate<Entity>Dto, <Entity>GetListInput
- App services: <Entity>AppService : ApplicationService, implements I<Entity>AppService
- Repositories: IRepository<Entity, Guid>
- Permissions: VctoonPermissions.<Feature>.<Action>

How to implement a new feature (high-level)

1. Domain: entities, value objects, domain services, error codes
2. EF: mappings, migrations
3. Application.Contracts: DTOs + app service interface + permissions
4. Application: app service implementation + mappings + validation
5. HttpApi: controller to expose app service if not using dynamic proxy
6. Web: Razor Pages for UI + authorization + localization
7. Tests: unit/integration as appropriate

Review checklist for changes

- Permissions defined and enforced
- DTOs mapped correctly; no entity leakage to UI/API
- Async all the way; cancellation tokens where useful
- Localization added for user-facing text
- Exceptions use BusinessException with codes
- Unit/integration tests added/updated

Git commit instructions

- Format
  <type>: <short summary>

  <detailed description within 100 characters>

  Types: feat | fix | style | perf | refactor | revert | test | docs | chore | workflow | ci | types | wip

- Use English; keep line 3 �� 100 chars

Repository-specific guidance for Copilot

- Prefer Razor Pages examples over MVC/Blazor
- Use ABP 9.3.x APIs and patterns; avoid obsolete members
- Use AutoMapper via IObjectMapper; do not introduce manual mapping unless necessary
- Use IRepository and IQueryable extensions for queries; avoid raw DbContext in app layer
- Follow the solution layout and place code in the correct project by responsibility
