# Changelog

All notable changes to InventoryApp are documented here.
Versioning follows [Semantic Versioning](https://semver.org/).

---

## [0.1.0] — 2026-08-08

### Foundation — Phases 1 & 2

#### Added — Phase 1: Solution Architecture
- Four .NET 8 projects: `Inventory.Api`, `Inventory.Core`, `Inventory.Infrastructure`, `Inventory.Shared`
- Project references and NuGet dependencies configured
- `Program.cs` with DI wiring, Swagger, CORS, camelCase JSON, null-omit serialisation
- `ExceptionMiddleware` — global unhandled exception handler
- `BaseEntity` — base class providing `Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- `IApplicationDbContext` — DbContext abstraction exposed to Core layer
- `IRepository<T>` interface and `Repository<T>` generic EF Core implementation
- `ApplicationDbContext` with `StampAuditFields()` auto-stamping UTC timestamps on save
- `ApiResponse<T>`, `PagedList<T>`, `Result<T>` shared utilities
- `appsettings.json` with connection string placeholder and JWT config placeholder
- `.gitignore` for .NET projects

#### Added — Phase 2: Database & EF Core Foundation
- **3 enums** in `Inventory.Core/Enums/`:
  - `StoreRequestStatus` (Pending, Approved, Rejected, Issued, Cancelled)
  - `TransactionType` (StockIn, StockOut, Adjustment)
  - `ReferenceType` (Purchase, StoreRequest, StockIssue)
- **14 domain entities** in `Inventory.Core/Entities/`:
  - `Role`, `User` — identity
  - `Category`, `Unit`, `Department`, `Supplier`, `Product` — master data
  - `Purchase`, `PurchaseItem` — purchasing
  - `StoreRequest`, `StoreRequestItem` — requisition workflow
  - `StockIssue`, `StockIssueItem` — stock dispatch
  - `StockTransaction` — immutable audit trail
- **14 EF Core configurations** in `Inventory.Infrastructure/Data/Configurations/`:
  - SQL Server column types, max lengths, decimal precision
  - Unique indexes, filtered indexes, composite indexes
  - FK relationships with explicit delete behaviors (Cascade on line items, Restrict on all masters and audit trail)
  - Role seed data
- **`ApplicationDbContext`** updated with 14 `DbSet<T>` properties
- **Migration**: `20260808151940_InitialCreate` — 14 tables verified
- **Seed data**: Roles — Admin (1), StoreKeeper (2), KitchenUser (3)
- Solution build: **0 errors**

#### Repository
- GitHub repository initialised: `https://github.com/haroon9013/InventoryApp`
- Initial commit pushed to `main` branch

---

## Upcoming

### [0.2.0] — Phase 3: Authentication *(in planning)*
- JWT login endpoint
- BCrypt password verification
- Role-based authorization on API controllers
- Initial admin user seeding

---

*For the full development roadmap see [`docs/DEVELOPMENT_PLAN.md`](DEVELOPMENT_PLAN.md).*
