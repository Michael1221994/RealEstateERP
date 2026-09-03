# Environment Setup

Target stack (locked in [`../status/decisions.md`](../status/decisions.md)):

| Concern | Choice |
| --- | --- |
| Runtime | .NET 8 (SDK 8.x; SDK 9.x also builds these projects) |
| Web framework | ASP.NET Core Web API |
| ORM | EF Core 8 + Npgsql (PostgreSQL) |
| Database | PostgreSQL 15+ |
| Object storage (documents) | MinIO (S3-compatible) |
| Messaging/background jobs | Hangfire (optional, available in `Infrastracture.Base.API`) |
| Auth | JWT bearer (local issuer for v1; Keycloak multi-realm option available) |

> The solution is **already scaffolded** (2026-09-03): `RealEstateERP.sln` + `src/RealEstateERP.{Core,Infrastructure,API}` + `tests/RealEstateERP.Tests`. The commands below are the reference for how it was created.

## 1. Prerequisites

1. **.NET SDK 8.0** (or newer) — verify with `dotnet --version`.
2. **PostgreSQL 15+** — local install, Docker, or a dev server.
3. **MinIO** (or any S3-compatible store) for document storage. Docker:
   ```bash
   docker run -d --name minio -p 9000:9000 -p 9001:9001 \
     -e "MINIO_ROOT_USER=minioadmin" -e "MINIO_ROOT_PASSWORD=minioadmin" \
     minio/minio server /data --console-address ":9001"
   ```
4. *(Optional)* Git + a GitHub/GitLab account for the repo.

## 2. Solution layout (target)

```
RealEstateERP.sln
Infrastructure/
  Infrastracture.Base/          # shared contracts & base types (already imported)
  Infrastracture.Base.API/      # web concerns: auth wrapper, middleware (already imported)
  Infrastracture.Base.EF/       # EF implementations, MinIO, PDF, ERMS (already imported)
Apps/Library/SES.WINSSAS.Common/  # dependency of the above (already imported)
src/
  RealEstateERP.Core/           # domain: entities, feature contracts & MediatR handlers
  RealEstateERP.Infrastructure/ # app DbContext, repositories, integrations, DI
  RealEstateERP.API/            # Program.cs, controllers
tests/
  RealEstateERP.Tests/          # xUnit tests
```

Scaffolding commands (once decided):

```bash
dotnet new sln -n RealEstateERP
dotnet new webapi -n RealEstateERP.API -o src/RealEstateERP.API --use-controllers
dotnet new classlib -n RealEstateERP.Core -o src/RealEstateERP.Core
dotnet new classlib -n RealEstateERP.Infrastructure -o src/RealEstateERP.Infrastructure
dotnet new xunit -n RealEstateERP.Tests -o tests/RealEstateERP.Tests
dotnet sln add src/RealEstateERP.API src/RealEstateERP.Core src/RealEstateERP.Infrastructure tests/RealEstateERP.Tests
```

Project references (mirror the pattern used by `SES.WINSSAS` apps):

- `RealEstateERP.Core` → `Infrastracture.Base`, `Infrastracture.Base.EF`, `SES.WINSSAS.Common`
- `RealEstateERP.Infrastructure` → `RealEstateERP.Core`, `Infrastracture.Base.EF`
- `RealEstateERP.API` → `RealEstateERP.Core`, `RealEstateERP.Infrastructure`, `Infrastracture.Base.API`
- `RealEstateERP.Tests` → `RealEstateERP.Core`, `RealEstateERP.Infrastructure`

## 3. Database setup

**Local dev instance (Docker):**

```bash
docker run --name postgres-erp -e POSTGRES_USER=RealEstateERP -e POSTGRES_PASSWORD=excelsior -e POSTGRES_DB=postgres -p 5433:5432 -d postgres
docker exec postgres-erp psql -U RealEstateERP -d postgres -c "CREATE DATABASE realestate_erp;"
```

Connection string (also supports the agency's existing host pattern — server, port, database, user, password):

```
Host=localhost;Port=5433;Database=realestate_erp;Username=RealEstateERP;Password=excelsior
```

**The connection string is not committed.** `appsettings.json` ships with empty placeholders — real values live in `dotnet user-secrets` (dev, per machine) or environment variables. On a fresh machine, store this dev value once:

```bash
dotnet user-secrets init --project src/RealEstateERP.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=realestate_erp;Username=RealEstateERP;Password=excelsior" --project src/RealEstateERP.API
```

## 4. Environment variables / configuration

Configuration lives in `appsettings.json` + environment overrides, following the conventions already used by the imported infrastructure (sections are bound with `Configuration.Bind` in the app's `DependencyInjection`). **Secret values are never committed** — `appsettings.json` holds placeholders only (the file's JSON comments name the keys); supply them via user-secrets in dev or via `Section__Key` environment variables (which take precedence) anywhere else:

| Key | Purpose | Example |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | PostgreSQL | `Host=localhost;Port=5433;Database=realestate_erp;...` |
| `Jwt:SecretKey` | Signing key for local-issued tokens (base64) | |
| `Jwt:Issuer` / `Jwt:Audience` | Token issuer/audience | |
| `Jwt:Schemes` | *(optional, Keycloak)* array of realm schemes — see `Program.cs` pattern in `SES.WINSSAS.Entitlement.API` | |
| `Minio:EndPoint` | MinIO host | `localhost:9000` |
| `Minio:AccessKey` / `Minio:SecretKey` | MinIO credentials | `minioadmin` |
| `Minio:IsSecure` | Use TLS for MinIO | `false` in dev |
| `Minio:Bucket` | Default document bucket | `realestate-documents` |
| `TaxRates:CapitalGainsRate` | CGT % (decimal) | `0.15` |
| `TaxRates:StampDutyRate` | Stamp duty % | `0.02` |
| `TaxRates:TransferTaxRate` | Transfer tax % | `0.02` |
| `Commission:AgencyRate` | Agency commission % | `0.02` |
| `Commission:DelalaRate` | Delala commission % | `0.01` |
| `ExchangeRate:DefaultCurrency` | Base currency | `ETB` |

## 5. Migrations

EF Core migrations live in `RealEstateERP.Infrastructure` (add the design-time package to the API project if needed):

```bash
# Local tool (restore once per clone)
dotnet tool restore

# First time
dotnet ef migrations add InitialCreate --project src/RealEstateERP.Infrastructure --startup-project src/RealEstateERP.API
dotnet ef database update --project src/RealEstateERP.Infrastructure --startup-project src/RealEstateERP.API

# After model changes
dotnet ef migrations add <Name> --project src/RealEstateERP.Infrastructure --startup-project src/RealEstateERP.API
```

The initial migration (`InitialAuth`) already exists. Seed data is applied by `DbSeeder.SeedAdminAsync` at startup (idempotent). Additional lookup/tax-rate seed data (F-INF-03) will extend the seeder — do **not** rely on `EnsureCreated`.

## 6. Running

Before first run on a machine, make sure the required secrets are set (see section 3 and below — startup fails fast if `DefaultConnection` or `Jwt:SecretKey` are empty):

```bash
cd src/RealEstateERP.API
dotnet user-secrets set "Jwt:SecretKey" "<base64 signing key>"
dotnet user-secrets set "Seed:AdminPassword" "<initial admin password>"
# e.g. dev values: Jwt:SecretKey base64 of a 32+ byte key, Seed:AdminPassword = Admin@123
cd ../..
dotnet run --project src/RealEstateERP.API
```

Swagger: `http://localhost:<port>/swagger` (port from `Properties/launchSettings.json`). On startup the API runs `MigrateAsync` and seeds the initial administrator from the `Seed:*` config section (dev default `admin` / `Admin@123` — set via user-secrets).

## 7. Verification checklist for a new dev machine

- [ ] `dotnet build` on the whole solution succeeds (Infrastructure projects included)
- [ ] `dotnet user-secrets` hold `ConnectionStrings:DefaultConnection`, `Jwt:SecretKey`, `Seed:AdminPassword`
- [ ] PostgreSQL reachable and `realestate_erp` exists
- [ ] `dotnet ef database update` applies migrations
- [ ] MinIO reachable (`curl http://localhost:9000/minio/health/live`)
- [ ] Login works and returns a JWT
