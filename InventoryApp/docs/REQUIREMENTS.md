# Requirements

> Status key: ✅ Agreed | 🔜 Deferred | ❌ Out of scope

---

## Functional Requirements

### FR-01 — User & Role Management
| # | Requirement | Status |
|---|---|---|
| FR-01.1 | The system shall support three roles: Admin, StoreKeeper, KitchenUser | ✅ |
| FR-01.2 | Each user shall be assigned exactly one role | ✅ |
| FR-01.3 | Users shall authenticate via username and password | 🔜 Phase 3 |
| FR-01.4 | Passwords shall be stored as BCrypt hashes, never plain text | 🔜 Phase 3 |
| FR-01.5 | Admin users shall be able to create, deactivate, and update users | 🔜 Phase 4 |

### FR-02 — Master Data
| # | Requirement | Status |
|---|---|---|
| FR-02.1 | Admin shall manage product categories | 🔜 Phase 4 |
| FR-02.2 | Admin shall manage units of measure (Kg, Gram, Litre, ml, Piece, Packet, Box) | 🔜 Phase 4 |
| FR-02.3 | Admin shall manage departments (Kitchen, Store, Bakery, Bar) | 🔜 Phase 4 |
| FR-02.4 | Admin shall manage suppliers | 🔜 Phase 4 |
| FR-02.5 | Admin shall manage products with a single consolidated stock balance | ✅ Domain |
| FR-02.6 | Products shall have a minimum stock threshold for low-stock alerts | ✅ Domain |
| FR-02.7 | Master records shall be deactivated, not physically deleted, when historical data exists | ✅ |

### FR-03 — Purchasing
| # | Requirement | Status |
|---|---|---|
| FR-03.1 | StoreKeeper shall record a purchase (GRN) from a supplier | 🔜 Phase 5 |
| FR-03.2 | A purchase shall have one or more line items (product, quantity, price) | ✅ Domain |
| FR-03.3 | Recording a purchase shall increase the product's stock balance | 🔜 Phase 5 |
| FR-03.4 | A StockTransaction (StockIn) shall be recorded for every purchase line | 🔜 Phase 5 |
| FR-03.5 | Purchase records shall not be editable after creation | ✅ |

### FR-04 — Store Requests
| # | Requirement | Status |
|---|---|---|
| FR-04.1 | A KitchenUser shall raise a Store Request for required items | 🔜 Phase 6 |
| FR-04.2 | A Store Request shall have a status: Pending, Approved, Rejected, Issued, Cancelled | ✅ Domain |
| FR-04.3 | A StoreKeeper shall approve or reject a Store Request | 🔜 Phase 6 |
| FR-04.4 | Approval and rejection shall record the user and timestamp | ✅ Domain |

### FR-05 — Stock Issue
| # | Requirement | Status |
|---|---|---|
| FR-05.1 | A StoreKeeper shall issue stock against an approved Store Request | 🔜 Phase 7 |
| FR-05.2 | An ad-hoc issue without a preceding Store Request shall be permitted | ✅ Domain |
| FR-05.3 | Each issue line shall record the product, quantity, and optional purpose | ✅ Domain |
| FR-05.4 | Issuing stock shall reduce the product's stock balance | 🔜 Phase 7 |
| FR-05.5 | A StockTransaction (StockOut) shall be recorded for every issue line | 🔜 Phase 7 |

### FR-06 — Stock Adjustments
| # | Requirement | Status |
|---|---|---|
| FR-06.1 | A StoreKeeper/Admin shall record manual stock adjustments (e.g. damage, count correction) | 🔜 Phase 8 |
| FR-06.2 | Every adjustment shall create a StockTransaction (Adjustment) with a reason | 🔜 Phase 8 |

### FR-07 — Inventory Principle
| # | Requirement | Status |
|---|---|---|
| FR-07.1 | A product shall have ONE consolidated stock balance regardless of department | ✅ |
| FR-07.2 | Purpose of consumption shall be recorded on StockIssueItem for reporting only | ✅ Domain |
| FR-07.3 | StockTransaction is the permanent, immutable movement history | ✅ |
| FR-07.4 | Product.CurrentStock provides fast access; StockTransaction is the source of truth | ✅ |

### FR-08 — Reports *(Planned)*
| # | Requirement | Status |
|---|---|---|
| FR-08.1 | Stock balance report per product | 🔜 Phase 9 |
| FR-08.2 | Stock ledger (movement history per product with date range) | 🔜 Phase 9 |
| FR-08.3 | Consumption by department and purpose | 🔜 Phase 9 |
| FR-08.4 | Purchase history by supplier | 🔜 Phase 9 |
| FR-08.5 | Low-stock alert report | 🔜 Phase 9 |

### FR-09 — Mobile App *(Planned)*
| # | Requirement | Status |
|---|---|---|
| FR-09.1 | Flutter Android app for KitchenUsers to raise and track store requests | 🔜 Phase 10 |
| FR-09.2 | Flutter app for StoreKeepers to process requests and record issues | 🔜 Phase 10 |

---

## Non-Functional Requirements

| # | Requirement | Notes |
|---|---|---|
| NFR-01 | All timestamps shall be stored in UTC | `DateTime.UtcNow` used throughout |
| NFR-02 | Passwords shall never be stored in plain text | BCrypt hashing — Phase 3 |
| NFR-03 | Stock transaction history shall not be physically deleted | All TX FKs use `Restrict` delete |
| NFR-04 | Foreign keys shall use `Restrict` on master data to protect history | Enforced in EF configurations |
| NFR-05 | Decimal precision: quantities `(18,3)`, prices and amounts `(18,2)` | Configured in EF |
| NFR-06 | API responses shall use camelCase JSON | Configured in `Program.cs` |
| NFR-07 | Null fields shall be omitted from JSON responses | `JsonIgnoreCondition.WhenWritingNull` |
| NFR-08 | The API shall document all endpoints via Swagger/OpenAPI | Swashbuckle configured |
| NFR-09 | All SQL constraints (unique, FK, indexes) shall be enforced at the database level | Done via EF configurations |
| NFR-10 | Connection strings shall not be hard-coded | Placeholder in `appsettings.json` |
