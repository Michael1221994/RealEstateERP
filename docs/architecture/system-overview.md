# System Overview

## Architecture style: modular monolith

**Decision: build a modular monolith** for this ERP, not microservices.

Rationale: the agency's existing platform (the `winssas-admin-services` solution this Infrastructure was taken from) is a 6-service microservice monorepo — but that suits a large public-sector entitlement system with independent deployment needs. This ERP targets a single agency, ~50 concurrent users, and one database; microservices would add operational cost with no benefit. We keep the **same code conventions** the team already knows (Clean-Architecture-flavored layering, feature folders, `Response<T>` envelopes, MediatR) inside one deployable.

Module boundaries are enforced by **feature folders + project references**, not by separate processes:

```
RealEstateERP.API            → HTTP, auth, composition root
  └─ uses MediatR commands/queries
RealEstateERP.Core           → domain: entities, feature contracts (interfaces), handlers, DTOs
RealEstateERP.Infrastructure → app DbContext, repository implementations, MinIO/HTTP integrations, DI
Infrastracture.Base(.API/.EF) → shared building blocks (imported, unchanged)
```

## Tech stack

| Layer | Technology | Notes |
| --- | --- | --- |
| Runtime | .NET 8 / ASP.NET Core | net8.0 targets in imported projects |
| Web | Web API controllers | thin controllers per existing convention |
| Use-case orchestration | MediatR | commands/queries + handlers in Core |
| ORM / data access | EF Core 8 + Npgsql | PostgreSQL 15+ |
| Cross-cutting contracts | `Infrastracture.Base` | `Response<T>`, `IRepository`, `IUnitOfWork`, `ISpecification`, `IDocumentService`, `IPDFGenerator`, base entities |
| EF implementations | `Infrastracture.Base.EF` | `GenericRepository`, `UnitOfWork`, `MinioDocumentService`, `ITextPdfGenerator` |
| Web plumbing | `Infrastracture.Base.API` | `InAppIdentityAuthorizedController`, exception/security-headers/rate-limit middleware, workflow/role authorization |
| Document storage | MinIO (S3-compatible) | via `IDocumentService` |
| PDF generation | iText (+ bundled Amharic font `nyala.ttf`) | agreements, tax vouchers, receipts |
| Auth | JWT bearer (local issuer v1; Keycloak multi-realm optional) | roles via claims; default-deny policy |
| Background jobs | Hangfire (available in Base.API) | payment reminders, report generation later |
| Tests | xUnit + EF InMemory (+ optional Testcontainers Postgres) | |

## Dependency & data-flow rules

```
Controller (API) ── MediatR Command/Query ──▶ Handler (Core)
Handler ──▶ feature repository interface (Core) ──▶ implementation (Infrastructure) ──▶ EF/Postgres
Handler ──▶ IRepository / IUnitOfWork (shared)   ──▶ GenericRepository / UnitOfWork (shared EF)
Handler ──▶ IDocumentService (shared contract)   ──▶ MinioDocumentService (shared impl)
```

Every service/handler returns `Response<T>` (`responseStatus: Success|Error|Warning|Info|NotFound` + `message`/`messageCode` + `data`). Controllers add auth context (user/role claims) to commands and pass them to MediatR — no business logic in controllers.

**Pragmatic caveat (inherited from the source platform):** `Core` references `Infrastracture.Base.EF` directly and domain entities derive from shared `BaseEntity`; the "infrastructure layer" is a shared *library*, not a set of interfaces Core depends on for inversion. App-specific persistence (`RealEstateERP.Infrastructure`) is where EF `DbContext`, per-feature repositories, and external HTTP clients live. Keep new code consistent with this — do not introduce a strict ports-and-adapters refactor of the shared libraries.

## Directory layout (target)

```
src/
  RealEstateERP.API/
    Program.cs
    Controllers/
      AuthController.cs  PropertiesController.cs  ClientsController.cs
      SalesAgreementsController.cs  RentalsController.cs  PaymentsController.cs
      CommissionsController.cs  DocumentsController.cs  ReportsController.cs
  RealEstateERP.Core/
    Models/                      # domain entities (Property, Client, Agreement, ...)
    Features/
      <FeatureName>/
        Contract/{Command,Query,Service,Repository}/
        Handler/{Command,Query,Service}/
        DTOs/
    Enums/
  RealEstateERP.Infrastructure/
    Context/RealEstateDbContext.cs
    Dependency/DependencyInjection.cs   # AddInfrastructure(configuration)
    Repository/                          # per-feature EF repositories
    Services/                            # concrete integrations (exchange-rate, exports)
Infrastructure/                          # shared libraries (imported)
  Infrastracture.Base / .API / .EF
Apps/Library/SES.WINSSAS.Common
tests/
  RealEstateERP.Tests/
docs/                                    # this documentation
```

## Module map (from the BUC/BRD)

| Module | Owns | Major dependencies |
| --- | --- | --- |
| Auth & Users | users, roles, login, audit | — |
| Property | listings, tenure, documents, status lifecycle | Client, Document |
| Client | buyers/sellers/tenants/landlords/Delalas, TIN, interactions | Document |
| Sales | agreements, payment schedules, taxes/commissions calc, title-transfer workflow | Property, Client, Document, Finance |
| Rental | leases, rent collection, maintenance requests | Property, Client, Document |
| Finance | payments, GL, reconciliation, ERCA/tax reporting | Sales, Rental |
| Reporting | sales/occupancy/P&L/tax reports, Excel/PDF | all modules (read-only) |
| Document | upload/retrieve/scans (title deeds, agreements, receipts) | MinIO |

## Cross-cutting concerns

- **Auth/authorization**: default-deny; `[Authorize(Roles = "...")]`; workflow guards for state changes (e.g., only Legal can mark title transferred).
- **Localization**: Amharic supported in generated documents (iText + `nyala.ttf`); UI/calendar localization is Phase 2.
- **Money**: decimal everywhere; amounts stored with currency + captured exchange rate (see `database-schema.md`).
- **Error handling**: `GlobalExceptionHandlingMiddleware` from `Infrastracture.Base.API` is the outermost middleware; safe messages with trace IDs, full detail logged server-side.
