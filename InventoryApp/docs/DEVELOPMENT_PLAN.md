# Development Plan

## Approach

InventoryApp is built in structured phases. Each phase is merged to `main` only when the build is clean and the phase objectives are verified. No phase skips another phase's dependencies.

---

## Phase 1 — Solution Architecture ✅ COMPLETE

**Objective:** Establish the production-ready multi-project solution skeleton.

**Delivered:**
- Four .NET 8 projects: `Inventory.Api`, `Inventory.Core`, `Inventory.Infrastructure`, `Inventory.Shared`
- Project references configured (Core ← Infrastructure ← Api, Shared ← all)
- NuGet packages: EF Core, SQL Server, AutoMapper, FluentValidation, Swagger, JwtBearer, BCrypt
- `Program.cs` with DI wiring, middleware pipeline, CORS, Swagger
- `ExceptionMiddleware` — global error handler
- `BaseEntity` — audit base class
- `IApplicationDbContext` — DbContext abstraction in Core
- `IRepository<T>` / `Repository<T>` — generic repository
- `ApplicationDbContext` — EF DbContext scaffold
- `ApiResponse<T>`, `PagedList<T>`, `Result<T>` in Shared
- `.gitignore`, `appsettings.json` with connection string placeholder

---

## Phase 2 — Database & EF Core Foundation ✅ COMPLETE

**Objective:** Model the complete hotel inventory domain and produce a verified SQL Server schema.

**Delivered:**
- 3 enums: `StoreRequestStatus`, `TransactionType`, `ReferenceType`
- 14 domain entities: `Role`, `User`, `Category`, `Unit`, `Department`, `Supplier`, `Product`, `Purchase`, `PurchaseItem`, `StoreRequest`, `StoreRequestItem`, `StockIssue`, `StockIssueItem`, `StockTransaction`
- 14 EF Core `IEntityTypeConfiguration<T>` classes with relationships, indexes, decimal precision, and delete behaviors
- `ApplicationDbContext` updated with all 14 `DbSet<T>` properties
- Migration: `20260808151940_InitialCreate` — 14 tables, all constraints, all indexes
- Seed data: Roles (Admin=1, StoreKeeper=2, KitchenUser=3)
- Solution build: 0 errors

---

## Phase 3 — Authentication 🔜 NEXT

**Objective:** Implement JWT-based authentication and role-based authorization.

**Planned scope:**
- `POST /api/auth/login` — validate credentials, return JWT
- `POST /api/auth/refresh` — refresh token (if refresh tokens are implemented)
- BCrypt password verification
- JWT token generation with role claims
- `[Authorize]` and `[Authorize(Roles = "...")]` applied to all controllers
- Activate `app.UseAuthentication()` / `app.UseAuthorization()` in `Program.cs`
- Admin-only `POST /api/users` endpoint for user creation
- Initial admin user seeding (secure, not hardcoded plain-text password)

**Dependencies:** Phase 2 ✅

---

## Phase 4 — Master Data APIs

**Objective:** CRUD endpoints for all master data entities.

**Planned scope:**
- Categories, Units, Departments, Suppliers, Products
- Validation: FluentValidation for all request DTOs
- Soft delete (IsActive flag) for all master data
- Low-stock product listing endpoint
- AutoMapper profiles for entity ↔ DTO mapping

**Dependencies:** Phase 3

---

## Phase 5 — Purchasing

**Objective:** Record stock-in transactions via supplier purchases.

**Planned scope:**
- `POST /api/purchases` — create purchase (GRN) with line items
- Update `Product.CurrentStock` on purchase save
- Write `StockTransaction` (StockIn) for each line item
- `GET /api/purchases` — list with filtering/pagination
- `GET /api/purchases/{id}` — detail with line items

**Dependencies:** Phase 4

---

## Phase 6 — Store Requests

**Objective:** Department-to-store requisition workflow.

**Planned scope:**
- `POST /api/store-requests` — KitchenUser raises a request
- `PUT /api/store-requests/{id}/approve` — StoreKeeper approves
- `PUT /api/store-requests/{id}/reject` — StoreKeeper rejects
- `PUT /api/store-requests/{id}/cancel` — requester cancels
- Status transition validation
- Role-based authorization on each action

**Dependencies:** Phase 4

---

## Phase 7 — Stock Issue

**Objective:** Records physical stock dispatched from store to department.

**Planned scope:**
- `POST /api/stock-issues` — StoreKeeper issues stock (optionally linked to a StoreRequest)
- Update `Product.CurrentStock` on issue save
- Write `StockTransaction` (StockOut) for each issue line item (with Purpose)
- StoreRequest status auto-transitions to `Issued`
- Ad-hoc issue without a preceding StoreRequest supported

**Dependencies:** Phase 6

---

## Phase 8 — Stock Adjustments

**Objective:** Manual corrections for damage, physical count corrections, write-offs.

**Planned scope:**
- `POST /api/stock-adjustments` — Admin/StoreKeeper records an adjustment
- Update `Product.CurrentStock`
- Write `StockTransaction` (Adjustment) with mandatory reason
- Adjustment history endpoint

**Dependencies:** Phase 5

---

## Phase 9 — Reports

**Objective:** Read-only reporting endpoints for management and operations.

**Planned scope:**
- Current stock balances (all products / low-stock filter)
- Stock ledger per product (date range)
- Consumption by department and purpose
- Purchase history by supplier
- Store request status summary

**Dependencies:** Phase 7, Phase 8

---

## Phase 10 — Flutter Android App

**Objective:** Mobile interface for KitchenUsers and StoreKeepers.

**Planned scope:**
- Login screen with JWT authentication
- KitchenUser: raise store requests, view request status
- StoreKeeper: view pending requests, approve/reject, record issues
- Push notifications for request status changes *(TBD)*
- Offline-capable where feasible *(TBD)*

**Dependencies:** Phase 9 (all APIs complete)

---

## Phase 11 — Testing & Deployment

**Objective:** Automated test coverage and production deployment.

**Planned scope:**
- Unit tests for Core services (xUnit)
- Integration tests for API endpoints (WebApplicationFactory)
- CI/CD pipeline (GitHub Actions)
- Production deployment target (TBD — Azure App Service / IIS / Docker)
- `appsettings.Production.json` with secrets management

**Dependencies:** Phase 10

---

## Phase 12 — Future ERP Integration

**Objective:** Export InventoryApp data to an external ERP system.

**Planned scope:**
- Export purchase records in ERP-compatible format
- Export stock transaction ledger
- Webhook or scheduled job-based synchronisation
- Specific ERP target TBD (SAP / Tally / custom)

**Dependencies:** Phase 11

---

## Removed / Out of Scope

The following were considered and explicitly excluded from the current plan:

- Room management / housekeeping inventory
- Generic "Item" entities (replaced by domain-specific entities)
- Multi-branch / multi-property support
- Point-of-sale integration
- Recipe management
