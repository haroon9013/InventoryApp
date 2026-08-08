# Database Design

## Overview

- **Database**: SQL Server
- **ORM**: Entity Framework Core 8
- **Migration**: `20260808151940_InitialCreate`
- **Total tables**: 14
- **Seed data**: Roles (Admin, StoreKeeper, KitchenUser)

---

## Core Inventory Rule

> **A product has ONE consolidated stock balance, regardless of departments or consumption purposes.**

`Product.CurrentStock` holds the running total. `StockTransaction` records every movement as an immutable ledger entry.

**Example — Cinnamon (initial stock: 10 kg)**

| Document | Department | Purpose | Qty | Transaction |
|---|---|---|---|---|
| Purchase GRN-001 | — | — | +10 kg | StockIn |
| Issue SI-001 | Kitchen | Biryani | −2 kg | StockOut |
| Issue SI-001 | Kitchen | Chinese | −1 kg | StockOut |
| Issue SI-001 | Kitchen | Meals | −0.5 kg | StockOut |
| **Balance** | | | **6.5 kg** | |

The `Purpose` field is **consumption metadata for reporting**. It does **not** create separate stock balances per purpose.

---

## Entity Reference

### Role
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | Identity |
| `Name` | nvarchar(50) | Unique |
| `IsActive` | bit | |

**Seeded values**: Admin (1), StoreKeeper (2), KitchenUser (3)

---

### User *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | Identity |
| `FullName` | nvarchar(150) | Required |
| `UserName` | nvarchar(100) | Unique |
| `PasswordHash` | nvarchar(500) | BCrypt hash |
| `RoleId` | int FK → Roles | Restrict |
| `IsActive` | bit | |
| `CreatedAt` | datetime2 | UTC |
| `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` | int? | |
| `UpdatedBy` | int? | |

---

### Category *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `CategoryName` | nvarchar(100) | Filtered unique index (IsActive=1) |
| `Description` | nvarchar(250) | Nullable |
| `IsActive` | bit | |
| `CreatedAt` / `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` / `UpdatedBy` | int? | |

> Filtered unique index allows the same name to be reused after deactivation.

---

### Unit *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `UnitName` | nvarchar(100) | Unique |
| `ShortName` | nvarchar(20) | Unique |
| `IsActive` | bit | |
| `CreatedAt` / `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` / `UpdatedBy` | int? | |

Examples: Kg, Gram, Litre, ml, Piece, Packet, Box

---

### Department *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `DepartmentName` | nvarchar(100) | Unique |
| `Description` | nvarchar(250) | Nullable |
| `IsActive` | bit | |
| `CreatedAt` / `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` / `UpdatedBy` | int? | |

Examples: Kitchen, Store, Bakery, Bar

---

### Supplier *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `SupplierName` | nvarchar(200) | Required |
| `ContactPerson` | nvarchar(150) | Nullable |
| `Mobile` | nvarchar(20) | Nullable |
| `Address` | nvarchar(500) | Nullable |
| `GSTNumber` | nvarchar(20) | Nullable — not all suppliers are GST-registered |
| `IsActive` | bit | |
| `CreatedAt` / `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` / `UpdatedBy` | int? | |

---

### Product *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `ProductCode` | nvarchar(50) | Unique |
| `ProductName` | nvarchar(200) | Required |
| `CategoryId` | int FK → Categories | Restrict |
| `UnitId` | int FK → Units | Restrict |
| `CurrentStock` | decimal(18,3) | Running balance |
| `MinimumStock` | decimal(18,3) | Alert threshold |
| `LastPurchasePrice` | decimal(18,2) | Updated on purchase |
| `IsActive` | bit | |
| `CreatedAt` / `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` / `UpdatedBy` | int? | |

---

### Purchase *(plain class — immutable header)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `PurchaseNo` | nvarchar(30) | Unique |
| `SupplierId` | int FK → Suppliers | Restrict |
| `PurchaseDate` | datetime2 | |
| `TotalAmount` | decimal(18,2) | |
| `CreatedBy` | int FK → Users | Restrict, required |
| `CreatedAt` | datetime2 | UTC |

> No `UpdatedAt` — purchases are immutable after creation.

---

### PurchaseItem *(plain class — no audit)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `PurchaseId` | int FK → Purchases | **Cascade** |
| `ProductId` | int FK → Products | Restrict |
| `Quantity` | decimal(18,3) | |
| `Price` | decimal(18,2) | Unit price at time of purchase |
| `Amount` | decimal(18,2) | Quantity × Price |

---

### StoreRequest *(extends BaseEntity)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `RequestNo` | nvarchar(30) | Unique |
| `DepartmentId` | int FK → Departments | Restrict |
| `RequestedBy` | int FK → Users | Restrict |
| `RequestDate` | datetime2 | |
| `Status` | int (enum) | 0=Pending, 1=Approved, 2=Rejected, 3=Issued, 4=Cancelled |
| `ApprovedBy` | int? FK → Users | Nullable, Restrict |
| `ApprovedDate` | datetime2? | Nullable |
| `RejectedBy` | int? FK → Users | Nullable, Restrict |
| `RejectedDate` | datetime2? | Nullable |
| `Remarks` | nvarchar(500) | Nullable |
| `CreatedAt` / `UpdatedAt` | datetime2 | UTC |
| `CreatedBy` / `UpdatedBy` | int? | |

---

### StoreRequestItem *(plain class — no audit)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `StoreRequestId` | int FK → StoreRequests | **Cascade** |
| `ProductId` | int FK → Products | Restrict |
| `Quantity` | decimal(18,3) | |
| `Remarks` | nvarchar(500) | Nullable |

---

### StockIssue *(plain class — immutable header)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `IssueNo` | nvarchar(30) | Unique |
| `StoreRequestId` | int? FK → StoreRequests | Nullable, Restrict |
| `DepartmentId` | int FK → Departments | Restrict |
| `IssuedBy` | int FK → Users | Restrict |
| `ReceivedBy` | int? FK → Users | Nullable, Restrict |
| `IssueDate` | datetime2 | |
| `Remarks` | nvarchar(500) | Nullable |
| `CreatedAt` | datetime2 | UTC |

> `StoreRequestId` is nullable — ad-hoc issues without a preceding request are allowed.

---

### StockIssueItem *(plain class — no audit)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `StockIssueId` | int FK → StockIssues | **Cascade** |
| `ProductId` | int FK → Products | Restrict |
| `Quantity` | decimal(18,3) | |
| `Purpose` | nvarchar(200) | Nullable — consumption context (e.g. "Biryani") |
| `Remarks` | nvarchar(500) | Nullable |

---

### StockTransaction *(plain class — permanent audit trail)*
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `ProductId` | int FK → Products | **Restrict** |
| `TransactionType` | int (enum) | 0=StockIn, 1=StockOut, 2=Adjustment |
| `Quantity` | decimal(18,3) | Always positive — direction from TransactionType |
| `ReferenceType` | int? (enum) | Nullable — 0=Purchase, 1=StoreRequest, 2=StockIssue |
| `ReferenceId` | int? | PK of the source document |
| `DepartmentId` | int? FK → Departments | Nullable, Restrict |
| `Purpose` | nvarchar(200) | Nullable |
| `Remarks` | nvarchar(500) | Nullable |
| `TransactionDate` | datetime2 | |
| `CreatedBy` | int FK → Users | Restrict, required |
| `CreatedAt` | datetime2 | UTC |

> **StockTransaction records are never physically deleted.** All FKs use `Restrict`. This table is the source of truth for all stock movement history.

---

## Indexes Summary

| Table | Index | Type |
|---|---|---|
| Roles | `Name` | Unique |
| Users | `UserName` | Unique |
| Categories | `CategoryName` WHERE `IsActive=1` | Filtered Unique |
| Units | `UnitName`, `ShortName` | Unique (×2) |
| Departments | `DepartmentName` | Unique |
| Products | `ProductCode` | Unique |
| Purchases | `PurchaseNo` | Unique |
| StoreRequests | `RequestNo` | Unique |
| StoreRequests | `(Status, DepartmentId)` | Composite |
| StockIssues | `IssueNo` | Unique |
| StockIssues | `(DepartmentId, IssueDate)` | Composite |
| StockTransactions | `(ProductId, TransactionDate)` | Composite |
| StockTransactions | `TransactionDate` | Standard |

---

## Delete Behavior Policy

| Relationship | Behavior | Reason |
|---|---|---|
| PurchaseItem → Purchase | **Cascade** | Line items have no meaning without header |
| StoreRequestItem → StoreRequest | **Cascade** | Line items have no meaning without header |
| StockIssueItem → StockIssue | **Cascade** | Line items have no meaning without header |
| All other FK relationships | **Restrict** | Protect master data and audit history |
