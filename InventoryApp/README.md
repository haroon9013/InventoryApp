# InventoryApp

> **Hotel Inventory Management System** — Production-ready ASP.NET Core backend for managing hotel store inventory, purchasing, stock issues, and consumption tracking.

---

## Project Description

InventoryApp is a multi-tier .NET 8 Web API designed to manage the complete inventory lifecycle of a hotel operation. It tracks stock from supplier purchase through to departmental consumption, providing accurate audit trails and real-time stock visibility.

## Business Purpose

Hotel kitchens and stores operate across multiple departments (Kitchen, Bakery, Bar, etc.) that share a single central inventory. The core challenge is:

- Tracking **one consolidated stock balance per product** (e.g. Cinnamon = 10 kg total)
- Issuing stock to departments and recording **consumption purpose** for reporting (e.g. Biryani 2 kg, Chinese 1 kg)
- Maintaining an **immutable transaction audit trail**
- Preventing stock from going unaccounted

InventoryApp solves this without creating separate stock accounts per department or purpose.

---

## Current Status

| Phase | Description | Status |
|---|---|---|
| Phase 1 | Solution Architecture | ✅ Complete |
| Phase 2 | Database & EF Core Foundation | ✅ Complete |
| Phase 3 | Authentication (JWT) | 🔜 Next |
| Phase 4 | Master Data APIs | ⏳ Planned |
| Phase 5 | Purchasing | ⏳ Planned |
| Phase 6 | Store Requests | ⏳ Planned |
| Phase 7 | Stock Issue | ⏳ Planned |
| Phase 8 | Stock Adjustments | ⏳ Planned |
| Phase 9 | Reports | ⏳ Planned |
| Phase 10 | Flutter Android App | ⏳ Planned |
| Phase 11 | Testing & Deployment | ⏳ Planned |
| Phase 12 | ERP Integration | ⏳ Planned |

> **No APIs are live yet.** The backend foundation (entities, EF Core, migration) is complete. Authentication is the next milestone.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| API Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Validation | FluentValidation 11 |
| Object Mapping | AutoMapper 12 |
| API Documentation | Swashbuckle / Swagger |
| Authentication | JWT Bearer *(planned — Phase 3)* |
| Mobile App | Flutter Android *(planned — Phase 10)* |

---

## Architecture Overview

```
InventoryApp.sln
├── Inventory.Api           ← ASP.NET Core Web API (entry point)
├── Inventory.Core          ← Domain entities, interfaces, services, DTOs
├── Inventory.Infrastructure← EF Core DbContext, repositories, migrations
└── Inventory.Shared        ← Shared response types, pagination helpers
```

**Dependency flow:** `Api → Core ← Infrastructure`, `Shared ← all`

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for full details.

---

## User Roles

| Role | Description |
|---|---|
| **Admin** | Full system access — users, master data, reports |
| **StoreKeeper** | Manages purchasing, stock issues, adjustments |
| **KitchenUser** | Raises store requests, views own department's stock |

---

## Core Inventory Principle

> **A product has ONE stock balance, regardless of how many departments or purposes it serves.**

**Example — Cinnamon (10 kg total):**

| Issue | Department | Purpose | Quantity |
|---|---|---|---|
| SI-001 | Kitchen | Biryani | 2 kg |
| SI-001 | Kitchen | Chinese | 1 kg |
| SI-001 | Kitchen | Meals | 0.5 kg |

The `Purpose` field on `StockIssueItem` records consumption context for reporting. It does **not** create separate inventory accounts.

---

## Database Overview

14 domain entities across 4 domains:

- **Identity**: `Role`, `User`
- **Master Data**: `Category`, `Unit`, `Department`, `Supplier`, `Product`
- **Purchasing**: `Purchase`, `PurchaseItem`
- **Store Requests**: `StoreRequest`, `StoreRequestItem`
- **Stock Issues**: `StockIssue`, `StockIssueItem`
- **Audit Trail**: `StockTransaction`

See [`docs/DATABASE.md`](docs/DATABASE.md) for full entity documentation.

---

## How to Run the Backend

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (local or remote)
- `dotnet-ef` CLI tool

### 1. Clone the repository
```bash
git clone https://github.com/haroon9013/InventoryApp.git
cd InventoryApp
```

### 2. Configure the connection string
Edit `Inventory.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=InventoryAppDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3. Apply the database migration
```bash
dotnet ef database update --project Inventory.Infrastructure --startup-project Inventory.Api
```

### 4. Run the API
```bash
cd Inventory.Api
dotnet run
```

Swagger UI will be available at: `http://localhost:<port>`

---

## Development Workflow

### Branching Strategy
```
main          ← stable, production-ready
feature/*     ← new features (e.g. feature/phase-3-auth)
fix/*         ← bug fixes
```

### Commit Convention
```
feat:  new feature
fix:   bug fix
docs:  documentation only
refactor: code restructure, no logic change
test:  add or update tests
chore: tooling, config, dependencies
```

### Add a new migration
```bash
dotnet ef migrations add <MigrationName> \
  --project Inventory.Infrastructure \
  --startup-project Inventory.Api
```

---

## Future ERP Integration

InventoryApp is designed as a standalone system that can later export data to an ERP (e.g. SAP, Tally, custom ERP). The `StockTransaction` audit trail and structured reference numbers (`PurchaseNo`, `RequestNo`, `IssueNo`) are designed to support this integration.

See [`docs/DEVELOPMENT_PLAN.md`](docs/DEVELOPMENT_PLAN.md) — Phase 12.

---

## Documentation

| Document | Description |
|---|---|
| [`docs/PROJECT_OVERVIEW.md`](docs/PROJECT_OVERVIEW.md) | Business context and scope |
| [`docs/REQUIREMENTS.md`](docs/REQUIREMENTS.md) | Functional and non-functional requirements |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Solution architecture and project structure |
| [`docs/DATABASE.md`](docs/DATABASE.md) | Entity model and database design |
| [`docs/DEVELOPMENT_PLAN.md`](docs/DEVELOPMENT_PLAN.md) | Phased development roadmap |
| [`docs/CHANGELOG.md`](docs/CHANGELOG.md) | Release history |
