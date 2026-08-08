# Project Overview

## What Is InventoryApp?

InventoryApp is a Hotel Inventory Management System built on ASP.NET Core (.NET 8). It provides a structured backend for managing the complete lifecycle of hotel store inventory — from supplier purchasing through to departmental stock consumption.

---

## The Problem It Solves

Hotel operations typically involve a central store that supplies multiple departments: Kitchen, Bakery, Bar, housekeeping, and others. Without a dedicated system, common problems include:

- **No accurate stock balances** — inventory is tracked on paper or in spreadsheets
- **No audit trail** — it is impossible to reconstruct what was purchased, issued, or consumed and when
- **No consumption analysis** — management cannot see which departments or dishes drive the highest ingredient costs
- **Over-purchasing** — without minimum stock alerts, orders are made on intuition
- **No approval workflow** — departments take stock without approval from store management

InventoryApp addresses all of these systematically.

---

## Intended Users

| Role | Who They Are | What They Do |
|---|---|---|
| **Admin** | Management / IT administrator | Configure users, master data, view reports |
| **StoreKeeper** | Central store staff | Receive purchases, approve requests, issue stock, adjust inventory |
| **KitchenUser** | Department heads (kitchen, bakery, bar) | Raise store requests, view their own consumption history |

---

## Core Inventory Principle

> **One product = one stock balance.**

A product such as Cinnamon has a single quantity on hand (e.g. 10 kg). When it is issued to the Kitchen for multiple purposes (Biryani, Chinese, Meals), each issue line records the `Purpose`. This purpose information drives consumption reports without splitting the physical stock into multiple accounts.

This is intentional and critical to the design. Do not create separate stock balances per department or purpose.

---

## Scope — Current

The current scope covers the backend API for:

- Domain entity definition and database schema
- EF Core migrations and SQL Server integration
- Role-based access foundation (roles seeded, auth deferred to Phase 3)
- Purchasing, store request, and stock issue workflows *(domain model complete, APIs pending)*

## Scope — Planned

- JWT authentication and role-based authorization
- RESTful CRUD APIs for all master data and workflows
- Flutter Android mobile application for KitchenUsers and StoreKeepers
- Management reports (stock ledger, consumption by department/purpose, purchase history)
- ERP export integration

## Out of Scope

- Multi-property / multi-branch hotel support (not planned)
- Point-of-sale integration (not planned)
- Recipe management (not planned)

---

## Future ERP Integration

InventoryApp's structured reference numbers (`PurchaseNo`, `RequestNo`, `IssueNo`) and the immutable `StockTransaction` audit trail are designed to allow future data export to enterprise ERP systems (SAP, Tally, or a custom ERP). This integration is planned as Phase 12 and will not require changes to the core domain model.
