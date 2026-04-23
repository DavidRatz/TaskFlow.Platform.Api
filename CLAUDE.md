# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build
dotnet build CarWashFlow.Platform.sln

# Run all tests
dotnet test CarWashFlow.Platform.sln

# Run a single test project
dotnet test Tests/CarWashFlow.Platform.Infrastructure.Tests

# Run a single test by name
dotnet test Tests/CarWashFlow.Platform.Infrastructure.Tests --filter "Cancel_SetsStateToCanceledAndCompletesSaga"

# Run with Docker (API + PostgreSQL + RabbitMQ + Mailpit)
docker-compose -f compose.yaml up
```

**Build notes:**
- `TreatWarningsAsErrors=true` — all warnings are errors. Fix them, don't suppress.
- StyleCop + NetAnalyzers run automatically on build.
- .NET 10.0 SDK required (`global.json`).

## Architecture

Clean Architecture with 6 projects in `Src/` + 4 test projects in `Tests/`:

```
Domain          → Entities, repository interfaces, domain services (zero dependencies)
Application     → MediatR commands/queries, handlers, DTOs
Infrastructure  → Stripe, Rebus/RabbitMQ, JWT, email (Razor templates)
Persistence     → EF Core DbContext, repository implementations, migrations
Presentation    → Carter HTTP endpoints (minimal API style)
Api             → ASP.NET Core host, DI composition root
```

**Dependency rule:** Domain ← Application ← Infrastructure/Persistence ← Presentation ← Api

### Key Patterns

**CQRS via MediatR:** Commands and queries are `sealed record` types implementing `IRequest<T>`. Handlers are `sealed class` with primary constructor DI.

**Repository + Unit of Work:** Interfaces in Domain, implementations in Persistence. `IUnitOfWork.SaveChangesAsync()` for transactional saves.

**Rebus Saga:** `SubscriptionLifecycleSaga` orchestrates subscription lifecycle via Stripe webhooks + internal commands. Correlated by `StripeSubscriptionId`. Full documentation in `docs/subscription-lifecycle-saga.md`.

**Outbox Pattern:** `OutboxOutgoingStep` ensures Rebus messages are saved atomically with business data before sending to RabbitMQ.

**Carter Endpoints:** Each feature group has an `ICarterModule` with route groups, authorization policies, and endpoint filters.

**DI Registration:** Each layer has a `DependencyInjection.cs` extension method called from `Program.cs` (`AddInfrastructure`, `AddPersistence`, `AddApplication`, `AddPresentation`).

### Database

- **PostgreSQL** with EF Core (Npgsql provider)
- `ApiContext` inherits `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- Entity configurations via `IEntityTypeConfiguration<T>` (fluent API, explicit `ToTable()` names)
- Optimistic concurrency via PostgreSQL `xmin` column on all domain entities
- Auto-migration on startup (`context.Database.MigrateAsync()`)

### Authentication & Authorization

- JWT Bearer tokens (HMAC-SHA256), revoked tokens tracked in DB
- ASP.NET Core Identity for user management
- Dynamic permission-based policies via `IAuthorizationPolicyProvider`

### External Services

- **Stripe** (v47.4.0): subscription management, webhooks dispatched via Rebus
- **RabbitMQ**: message transport for Rebus, queue `carwashflow_queue`
- **Mailpit**: development SMTP server

## Testing

- **Framework:** xUnit + Moq
- **DB in tests:** SQLite in-memory (not PostgreSQL)
- Test projects mirror source structure: `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`, `Persistence.Tests`

## Coding Conventions

### Entities (Domain)

- Inherit from `Entity` base class (`Id: Guid`, `CreatedAt`, `UpdatedAt`)
- Two constructors: public parameterized + private parameterless (for EF)
- All properties use `private set` with explicit `SetXxx()` methods
- Validation via `Ardalis.GuardClauses` (`Guard.Against.NullOrWhiteSpace`, `Guard.Against.Default`, etc.)
- Navigation properties typed as `List<T>`
- Domain logic lives on the entity (e.g. `subscription.SetStripeStatus(...)`)

```csharp
public sealed class Station : Entity
{
    public Station(Guid id, Guid organizationId, string name) : base(id)
    {
        SetOrganizationId(organizationId);
        SetName(name);
    }

    private Station() { }

    public string Name { get; private set; } = string.Empty;

    public void SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
    }
}
```

### Repository Interfaces (Domain)

- Located in `[Feature]/Repositories/IXxxRepository.cs`
- All methods take `CancellationToken cancellationToken` (no default)
- Read methods: `GetByIdAsync`, `GetAllAsync`, `GetByXxxAsync`
- Write methods: `AddAsync` (Task), `Add` (void)
- No `Update` method — EF change tracking handles mutations
- `SaveChangesAsync` on `IUnitOfWork`, not on repository

### Repository Implementations (Persistence)

- Located in `[Feature]/Repositories/XxxRepository.cs`
- Primary constructor injects `ApiContext context`
- Read-only queries: use `AsNoTracking()` for performance
- Mutation queries: no `AsNoTracking()` (tracked by EF for SaveChanges)
- Use `context.Set<T>()` or `context.DbSetProperty` to access entities
- Include navigations with `.Include()` / `.ThenInclude()`

### Commands & Queries (Application)

- **Command:** `sealed record XxxCommand(...) : IRequest<XxxDto>`
- **Query:** `sealed record XxxQuery(...) : IRequest<XxxDto>`
- One command/query per file, one handler per file
- Handler is `sealed class` with primary constructor DI, implements `IRequestHandler<TRequest, TResponse>`
- Handler method is `Handle(request, cancellationToken)` (not `HandleAsync`)
- Folder structure: `[Feature]/Commands/[CommandName]/XxxCommand.cs` + `XxxCommandHandler.cs`

```csharp
public sealed record CreateStationCommand(
    Guid OrganizationId,
    string Name) : IRequest<StationDto>;

public sealed class CreateStationCommandHandler(
    IStationRepository stationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateStationCommand, StationDto>
{
    public async Task<StationDto> Handle(
        CreateStationCommand request, CancellationToken cancellationToken)
    {
        // ...
    }
}
```

### DTOs (Application)

- `sealed record` types in `[Feature]/Dtos/`
- Multiple related DTOs can live in a single file (e.g. `StationDtos.cs`)
- Naming: `XxxDto`, `XxxDetailsDto`, `XxxInfoDto`

### Exceptions (Application)

- Located in `Common/Exceptions/` and `[Feature]/Exceptions/`
- Types: `ValidationFailedException`, `ResourceNotFoundException`, `BadRequestException`, `ConflictException`, `UnauthorizedAuthException`
- Feature-specific: `SubscriptionNotFoundException`, `SubscriptionForbiddenException`

### Endpoints (Presentation)

- One `ICarterModule` per feature group
- Routes versioned: `/v1/stations`, `/v1/me/subscriptions`
- Endpoint filters for exception → HTTP status mapping
- Responses wrapped: `Results.Ok(new { data = result })`
- Authorization via `.RequireAuthorization(Permissions.XxxYyy)`
- Handler methods are `private static async Task<IResult>`

```csharp
public class StationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("v1/stations")
            .AddEndpointFilter<StationEndpointFilters>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization(Permissions.StationsRead)
            .WithName("Stations.GetById")
            .WithTags("Stations");
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStationByIdQuery(id), cancellationToken);
        return Results.Ok(new { data = result });
    }
}
```

### Requests (Presentation)

- Located in `[Feature]/Requests/XxxRequest.cs`
- Simple classes with `{ get; init; }` properties

### Entity Configurations (Persistence)

- One file per entity: `[Feature]/EntityConfigurations/XxxEntityConfiguration.cs`
- Always explicit `builder.ToTable("TableName")`
- Always `builder.HasKey(x => x.Id)`
- Relationships via fluent API (`HasOne`, `HasMany`, `OnDelete`)

### Events & Messaging (Rebus)

The project uses **Rebus** with RabbitMQ for asynchronous event-driven messaging. The pattern follows the reference project (ChatBot):

**Domain events** are defined in Domain, published in Application, handled in Infrastructure.

```
Domain:         [Feature]/Events/XxxEvent.cs          → record or class defining the event
Application:    command handler calls bus.SendLocal()  → publishes the event
Infrastructure: [Feature]/EventHandlers/XxxEventHandler.cs → IHandleMessages<XxxEvent>
```

**Event definitions** (`Domain/[Feature]/Events/`):
- Simple `record` for events with few fields: `public record AgentCreatedEvent(Guid AgentId);`
- `class` with `required` properties for events with many fields:

```csharp
public class UserRegisteredEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
}
```

**Publishing events** (in Application command handlers):
- Inject `IBus bus` via primary constructor
- After business logic + save, call `await bus.SendLocal(new XxxEvent(...));`

**Event handlers** (`Infrastructure/[Feature]/EventHandlers/XxxEventHandler.cs`):
- Implement `IHandleMessages<XxxEvent>` (Rebus)
- Method: `public async Task Handle(XxxEvent message)` (no CancellationToken)
- Primary constructor DI for dependencies

```csharp
public sealed class UserRegisteredEventHandler(
    IEmailService emailService,
    ILogger<UserRegisteredEventHandler> logger)
    : IHandleMessages<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent message)
    {
        // side effects: send email, create external resources, etc.
    }
}
```

**Stripe webhook events** are a special case — they don't originate from domain logic but from external webhooks:

```
HTTP POST /webhook/stripe
  → MediatR StripeHandleEventCommand
    → IStripeEventDispatcher.DispatchAsync()
      → EventUtility.ConstructEvent() (signature verification)
      → Dictionary lookup: Stripe event type → message factory
      → bus.SendLocal(message)
        → Rebus handler OR saga
```

- Webhook messages: `sealed record StripeXxxEvent(string EventJson)` in `Infrastructure/Stripe/Events/`
- Naming: `Stripe` + Stripe event type in PascalCase + `Event`
- Handlers parse JSON via `EventUtility.ParseEvent()` and cast `stripeEvent.Data?.Object`

**Saga commands** (`Application/Subscriptions/Messages/`):
- `sealed record` types sent from MediatR handlers via `bus.SendLocal()`
- Handled by the saga via `IHandleMessages<T>`

**Infrastructure commands** (`Infrastructure/WashCredits/Messages/`):
- `sealed record` types sent from the saga via `bus.SendLocal()` or `bus.Defer()` (delayed)
- Handled by dedicated Rebus handlers

**Saga** (`Infrastructure/Stripe/Sagas/SubscriptionLifecycleSaga.cs`):
- `IAmInitiatedBy<T>` for the first event that creates the saga instance
- `IHandleMessages<T>` for subsequent correlated events
- Correlation via `CorrelateMessages()` override
- State stored in `SubscriptionLifecycleSagaData`
- `MarkAsComplete()` to end the saga
- Full documentation in `docs/subscription-lifecycle-saga.md`

### EF Migrations

- Single migration folder: `Persistence/Migrations/`
- Generate: `dotnet ef migrations add MigrationName -p Src/CarWashFlow.Platform.Persistence -s CarWashFlow.Platform.Api`

### General

- Nullable reference types enabled (`Nullable: enable`)
- `sealed` on all classes and records that are not inherited
- Primary constructor DI everywhere (no field assignments)
- Feature-based folder organization, namespace follows folder structure
- `IDateTimeProvider` for testable time (never `DateTime.UtcNow` directly)
- `AssemblyReference.Assembly` for auto-discovery (MediatR, EF configurations)
