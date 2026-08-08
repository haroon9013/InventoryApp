# Architecture

## Solution Structure

```
InventoryApp/
├── InventoryApp.sln
├── Inventory.Api/              ← ASP.NET Core Web API (entry point)
├── Inventory.Core/             ← Domain layer
├── Inventory.Infrastructure/   ← Data access layer
└── Inventory.Shared/           ← Cross-cutting utilities
```

---

## Project Responsibilities

### Inventory.Api
**Type:** `Microsoft.NET.Sdk.Web`

The HTTP entry point. Responsible for:
- Request routing via ASP.NET Core controllers
- Middleware pipeline (exception handling, authentication, CORS)
- Swagger / OpenAPI documentation
- Dependency injection wiring (calls `AddApplicationServices()` and `AddInfrastructure()`)

**Key files:**
| File | Purpose |
|---|---|
| `Program.cs` | App bootstrap, DI registrations, middleware pipeline |
| `Extensions/ApplicationServiceExtensions.cs` | Registers AutoMapper, FluentValidation |
| `Extensions/SwaggerExtensions.cs` | Swagger/JWT bearer UI setup |
| `Middleware/ExceptionMiddleware.cs` | Global unhandled exception handler |
| `appsettings.json` | Connection string, JWT config (placeholder), logging |

**NuGet packages:**
- `Swashbuckle.AspNetCore` — Swagger UI
- `Microsoft.AspNetCore.Authentication.JwtBearer` — JWT middleware *(registered, not yet activated)*
- `FluentValidation.AspNetCore` — request validation
- `AutoMapper.Extensions.Microsoft.DependencyInjection` — object mapping
- `Microsoft.EntityFrameworkCore.Design` — EF Core CLI tooling

---

### Inventory.Core
**Type:** `Microsoft.NET.Sdk`

The domain layer. Has **no dependency on Infrastructure** — it defines contracts that Infrastructure implements.

Contains:
- **`Entities/`** — 14 domain entity classes + `BaseEntity`
- **`Enums/`** — `StoreRequestStatus`, `TransactionType`, `ReferenceType`
- **`Interfaces/`** — `IApplicationDbContext`, `IRepository<T>`, service interfaces
- **`DTOs/`** — Data Transfer Objects *(to be populated in service phases)*
- **`Services/`** — Business logic services *(to be populated in service phases)*
- **`Mapping/`** — AutoMapper profile
- **`Validation/`** — FluentValidation validators *(to be populated in service phases)*

**NuGet packages:**
- `Microsoft.EntityFrameworkCore` — EF Core abstractions (no SQL Server dependency)
- `AutoMapper` — mapping profiles
- `FluentValidation` — validators

---

### Inventory.Infrastructure
**Type:** `Microsoft.NET.Sdk`

The data access layer. Implements Core contracts using EF Core + SQL Server.

Contains:
- **`Data/ApplicationDbContext.cs`** — EF Core DbContext with 14 DbSets and `StampAuditFields()`
- **`Data/Configurations/`** — 14 `IEntityTypeConfiguration<T>` classes (one per entity)
- **`Migrations/`** — EF Core migration files
- **`Repositories/Repository.cs`** — Generic repository implementation
- **`Extensions/InfrastructureServiceExtensions.cs`** — Registers DbContext + Repository into DI

**NuGet packages:**
- `Microsoft.EntityFrameworkCore.SqlServer` — SQL Server provider
- `Microsoft.EntityFrameworkCore.Tools` — EF CLI tools (`migrations add`, `database update`)
- `BCrypt.Net-Next` — Password hashing *(ready for Phase 3)*

---

### Inventory.Shared
**Type:** `Microsoft.NET.Sdk`

Lightweight library with no business logic. Referenced by all other projects.

Contains:
- `Common/ApiResponse<T>` — standardised HTTP response wrapper
- `Common/PagedList<T>` — pagination result container
- `Common/Result` — operation result type

---

## Dependency Flow

```
Inventory.Api
    ├── → Inventory.Core        (domain contracts)
    ├── → Inventory.Infrastructure (DI registration only)
    └── → Inventory.Shared

Inventory.Core
    └── → Inventory.Shared

Inventory.Infrastructure
    ├── → Inventory.Core        (implements IApplicationDbContext, IRepository<T>)
    └── → Inventory.Shared
```

> **Core never depends on Infrastructure.** This is enforced by the project reference graph.

---

## EF Core Design

- **DbContext**: `ApplicationDbContext` (in Infrastructure)
- **Configuration pattern**: Separate `IEntityTypeConfiguration<T>` class per entity, auto-discovered via `ApplyConfigurationsFromAssembly()`
- **Audit stamping**: `StampAuditFields()` auto-sets `CreatedAt`/`UpdatedAt` on all `BaseEntity` subclasses during `SaveChanges`
- **BaseEntity**: `Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` — inherited by all mutable master entities
- **Plain classes**: `Purchase`, `StockIssue`, `StockTransaction`, `PurchaseItem`, `StoreRequestItem`, `StockIssueItem`, `Role` — do not inherit `BaseEntity` (intentional, see entity specs)

---

## Authentication *(Planned — Phase 3)*

- **Mechanism**: JWT Bearer tokens
- **Library**: `Microsoft.AspNetCore.Authentication.JwtBearer` *(already installed)*
- **Status**: Middleware stubs exist in `Program.cs` but are commented out
- **Implementation scope**: Login endpoint, token generation, `[Authorize]` attributes on controllers

```csharp
// Currently in Program.cs — will be activated in Phase 3:
// app.UseAuthentication();
// app.UseAuthorization();
```

---

## Flutter Mobile App *(Planned — Phase 10)*

- **Platform**: Android (Flutter)
- **Status**: Not started
- **Depends on**: Phase 3 (auth), Phase 4–7 (APIs)
- **Target users**: KitchenUsers (store requests) and StoreKeepers (approvals, issues)

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Core has no EF dependency | Keeps domain logic testable without a database |
| `StampAuditFields()` in DbContext | Consistent audit timestamps without decorating every service call |
| All master FK relationships use `Restrict` | Prevents accidental deletion of entities with historical data |
| `StockTransaction` has all-`Restrict` FKs | Audit trail must never be cascade-deleted |
| Enums stored as `int` | EF Core default; numeric storage is compact and SQL-friendly |
| Filtered unique index on `CategoryName` | Allows the same name to be reused after deactivation |
| `CurrentStock` on `Product` + `StockTransaction` ledger | Fast access balance + full historical reconstruction |
