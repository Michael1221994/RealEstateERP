# Real Estate Agency ERP — Backend

A backend ERP system for an Ethiopian real estate agency: property listings, client and Delala management, sales agreements with title-transfer workflows, rentals, payments, commissions, taxes, documents, and reporting — tailored to Ethiopian regulations and practice.

Built on **.NET 8 / ASP.NET Core** with **PostgreSQL** and a shared, pre-built infrastructure layer (see below).

## Source documents

- **Business Use Case (BUC) & BRD**: [`ERP_System_Ethiopian_Real_Estate_Agency.docx`](../../ERP_System_Ethiopian_Real_Estate_Agency.docx) — the authoritative requirements source. Key content, summarized:

  - **BUC UC-RE-001 — Process Property Sale and Transfer of Ownership**: the flagship workflow (12-step happy path). Buyer and Seller agree terms → system calculates taxes/commissions → agreement generated in Amharic or English → signed → down payment recorded → Legal Officer verifies title deed & checks encumbrances → documents prepared for Addis Ababa City Land Administration / Notary → stamp duty & transfer tax paid → government transfers title → final payment + capital gains voucher → property marked Sold → commissions released → post-sale documents generated.
  - **Business rules**: all money in ETB (USD converted at the daily National Bank of Ethiopia rate); down payment ≥ 10%; capital gains tax **15%** on seller's profit; stamp duty **2%** and transfer tax **2%** on the buyer, computed on the *higher* of government-assessed value or agreed price; agency commission **2%**; Delala commission **1%** when involved; Legal Officer must verify title authenticity and absence of encumbrances before transfer.
  - **Modules in scope**: Property, Client, Sales, Rental, Finance & Accounting, Reporting & Analytics, User Management, Document Management.
  - **Out of scope (Phase 1)**: client mobile app, bank API auto-confirmation, GIS mapping.
  - **Context**: leasehold tenure, multi-currency, local taxes (ERCA), informal brokers (*Delalas*), Amharic + Ethiopian calendar support.

## Using this documentation

**For humans:** start at [`../status/progress.md`](../status/progress.md) for where things stand, then browse [`../architecture/system-overview.md`](../architecture/system-overview.md) before touching code.

**For AI assistants:** see the navigation order in [`../README.md`](../README.md). Read `progress.md` and `decisions.md` first; they are the source of truth for project state and locked-in choices.

## Current status

> **Phase 1: Backend — foundation + Auth core delivered (2026-09-03).**
> Solution scaffolded; login/JWT/RBAC working with tests. Next: audit log, then Property & Client modules — see [`../status/progress.md`](../status/progress.md).

## Quick start

Requires .NET 8 SDK+ and PostgreSQL (see [`setup.md`](setup.md)). From the repo root:

```bash
dotnet build RealEstateERP.sln          # restores the imported Infrastructure too

dotnet tool restore                     # installs dotnet-ef (local tool)
dotnet ef database update --project src/RealEstateERP.Infrastructure --startup-project src/RealEstateERP.API

dotnet run --project src/RealEstateERP.API
```

The API applies migrations and seeds the initial administrator on startup (`Seed:*` config — dev default **admin / Admin@123**, change before any shared use). Swagger: `http://localhost:<port>/swagger`. Login via `POST /api/auth/login`.

Run tests:

```bash
dotnet test RealEstateERP.sln
```
