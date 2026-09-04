# Decision Log

Keep this file in date order. **Every new decision gets an entry with context + alternatives + consequences — including reversals.** When a decision changes, add a new entry; never rewrite history silently.

---

## D-001 · 2026-09-03 — Build bespoke .NET backend (not ERPNext/Odoo customization)

- **Context:** The BRD constraints mention open-source ERPs (ERPNext/Odoo) as a possible route.
- **Decision:** Build a custom backend on .NET 8 with the agency's existing shared infrastructure layer.
- **Alternatives:** Customizing ERPNext/Odoo; Node.js/Express; Django.
- **Why:** The team already owns and operates a .NET microservice platform (`winssas-admin-services`) whose `Infrastracture.Base*` libraries cover our cross-cutting needs (repository pattern, `Response<T>`, MinIO documents, auth middleware, PDF with Amharic font). Reuse minimizes risk and keeps skills consistent. A generic open-source ERP would fight the Ethiopian-specific workflow (Delalas, leasehold, tax split, title-transfer gates).
- **Consequences:** Faster start; obligations to keep the imported libraries unchanged where possible.

## D-002 · 2026-09-03 — Modular monolith, not microservices

- **Context:** BUC targets one agency, ~50 concurrent users; the source platform is microservices, so that pattern is familiar.
- **Decision:** One deployable modular monolith. Modules = feature folders + project boundaries inside one solution/process.
- **Why:** No independent scaling/team needs to justify 6+ services; a monolith is cheaper to operate and refactor, and MediatR + feature folders already give module discipline.
- **Consequences:** If modules later need independent deployment, the feature boundaries were designed to make extraction straightforward.

## D-003 · 2026-09-03 — Import and reuse `Infrastracture.Base` / `.Base.API` / `.Base.EF` (+ `SES.WINSSAS.Common`) as-is

- **Context:** Copied from `winssas-admin-services/SES.WINSSAS` into this repo (2026-09-03) to build the new project on top of.
- **Decision:** Keep the three projects at their original relative layout (`Infrastructure/…`, `Apps/Library/SES.WINSSAS.Common`) with **no renames or refactors** (their public namespaces include the typo "Infrastracture" — references must match).
- **Why:** Project references are relative-path-based; renaming breaks them. Verified: all three build with 0 errors on .NET 9 SDK.
- **Consequences:** Code review rule — treat `Infrastructure/` as vendored: changes only when truly needed, with D-log entries.

## D-004 · 2026-09-03 — PostgreSQL via EF Core + Npgsql

- **Context:** BRD requires 10k+ properties, reliability, ERCA exports; source platform uses Postgres.
- **Decision:** PostgreSQL 15+; EF Core 8 + Npgsql; migrations in `RealEstateERP.Infrastructure`.
- **Alternatives:** SQL Server (licensing), MySQL.
- **Consequences:** Npgsql quirks apply; use the same connection-string/env conventions as the source platform.

## D-005 · 2026-09-03 — MediatR commands/queries for use-case orchestration

- **Context:** Proven in the source platform's feature folders (`Contract/Command`, `Contract/Query`, `Handler/…`).
- **Decision:** Handlers in Core are the single place business rules execute; controllers stay thin.
- **Consequences:** Consistent flow Controller → MediatR → Handler → repository/services → `Response<T>`.

## D-006 · 2026-09-03 — `Response<T>` envelope on all endpoints

- **Context:** Existing convention; clients and middleware already understand it.
- **Decision:** All handlers/controllers return `Response<T>` with `responseStatus ∈ {Success, Error, Warning, Info, NotFound}`.
- **Consequences:** No bespoke error body; `SafeError`/trace ids for failures.

## D-007 · 2026-09-03 — Money as decimal, ETB ledger + original currency + captured FX rate

- **Context:** BUC: amounts in ETB; USD allowed and converted at daily NBE rate.
- **Decision:** Domain/DB money is `decimal`; every multi-currency payment/agreement stores `amount`, `currency`, `exchange_rate_to_etb`, and `amount_etb` snapshot at transaction time. Ledger (`GL entries`, `tax_calculations`) posts in ETB.
- **Why:** Floating point is forbidden for money; rates change daily so the *rate used* must be immutable per transaction (audit).
- **Consequences:** Schema keeps both original and ETB values (see `database-schema.md`).

## D-008 · 2026-09-03 — Documents: bytes in MinIO, metadata + sha256 in PostgreSQL

- **Context:** BUC document management; Risk R1 (forged title deeds).
- **Decision:** Use `IDocumentService`/`MinioDocumentService` from the imported layer; `documents` table stores metadata incl. integrity hash.
- **Consequences:** Uploads record uploader + timestamp; tamper detection possible via stored `sha256`.

## D-009 · 2026-09-03 — JWT auth: local symmetric-key issuer for v1, multi-realm Keycloak config supported

- **Context:** The agency already runs Keycloak-style multi-realm JWT auth in `SES.WINSSAS` (`Program.cs` dynamic realm selector). For a single-agency ERP with internal staff, a simple local issuer is enough.
- **Decision:** Issue our own JWTs (symmetric key, `Jwt:SecretKey`, bcrypt password hashes via BCrypt.Net-Next). Keep the `Infrastracture.Base.API` building blocks so adding Keycloak realms later is configuration, not rework.
- **Consequences:** Role claim set is ours (`role: Admin|Sales|Finance|Legal|PropertyManager|ITAdmin`); external parties (Delalas) are not system users in v1.

## D-010 · 2026-09-03 — State transitions enforced in Core with allowed-transition maps + history tables

- **Context:** BUC NFR "only Legal Officer can change status to Title Transferred"; auditability.
- **Decision:** Status enums + explicit transition validation in handlers; dedicated history/event tables for property status, title transfer, and audit log; actor recorded from claims.
- **Consequences:** UI/frontend can't bypass rules by calling arbitrary endpoints.

## D-011 · 2026-09-03 — Configurable tax & commission rates, never hard-coded

- **Context:** Risk R6 (government changes tax rates); BRD wants ERCA compliance.
- **Decision:** Rates seeded/configurable (`TaxRates:*`, `Commission:*` config; persisted snapshot per calculation).
- **Consequences:** `tax_calculations`/`commissions` rows snapshot the rate used — changing config affects only future calculations.

## D-012 · 2026-09-03 — Amharic support: PDF generation now, UI/calendar localization later

- **Context:** BUC NFR: Amharic for agreements/reports; Ethiopian calendar optional.
- **Decision:** Generated documents (agreements, vouchers, receipts) support Amharic immediately via iText + bundled `nyala.ttf` in `Infrastracture.Base.EF`. Full Amharic UI and Ethiopian calendar display are Phase 2.
- **Consequences:** Reports/PDFs carry `lang=am|en`; UI strings untouched for now.

## D-013 · 2026-09-03 — OPEN: seller cost basis for capital gains

- **Context:** CGT = 15% on *profit*; profit needs a seller cost basis. BUC/BRD do not define how it is tracked (original purchase price per seller? per property? acquired in ETB or USD?).
- **Decision:** **Not yet made** — flagged in `progress.md` as the one blocking design question. Provisional approach: record an acquisition cost on the property/seller and let Finance confirm/adjust at finalization (F-FIN-03).
- **Consequences:** F-SALES-02 computes a provisional CGT estimate until D-013 resolves.

## D-014 · 2026-09-03 — Phase 1 excludes offline mode, banking APIs, and mobile

- **Context:** BRD lists offline entry, bank auto-confirmation, and a client app as future scope.
- **Decision:** Ship Phase 1 online-only; payments keyed manually; no client-facing app. Park these in the Phase 2 backlog rather than designing half-supported sync now.
- **Why:** Avoid building a sync engine before core workflows are proven (Risk R8 mitigation is manual/process-level for now).
- **Consequences:** Risk R8 partially accepted for Phase 1; revisit at Phase 2 planning.

## D-016 · 2026-09-03 — Auth implementation: local symmetric JWT, claims-based RBAC, bcrypt, seeded admin

- **Context:** F-AUTH-01/02/03 built 2026-09-03 (see `progress.md`). D-009 left the choice of local vs Keycloak open for the actual code.
- **Decision:**
  - Single local-issuer JWT scheme (`Jwt:SecretKey` base64, `Issuer`, `Audience`, `ExpiryMinutes`), validated by `AddJwtBearer` in `Program.cs` and issued by `AccessTokenService` (Infrastructure). No refresh tokens in v1; tokens expire (default 8 h) and require re-login.
  - Claims: `sub`/`NameIdentifier`/`UserID` (user id), `ClaimTypes.Name` (username), **`ClaimTypes.Role`** (role) so `[Authorize(Roles=…)]` works unchanged, plus `fullName`/`email`. See D-018 for the id type + claim-based identification rule.
  - Passwords hashed with **BCrypt.Net-Next** (4.0.3, same as the source platform). Usernames normalized to lowercase at creation/login.
  - Default-deny `FallbackPolicy`; user management endpoints restricted to `Admin,ITAdmin`.
  - Initial admin seeded idempotently on startup from `Seed:*` configuration (dev default `admin`/`Admin@123` — override in any shared environment).
- **Consequences:** Switching to Keycloak later means adding realm schemes in `Program.cs` (pattern exists in `SES.WINSSAS`); role claims must keep the same names. Token revocation only via expiry/disable flag at login time — the account-disable check runs on every login, not per request.

## D-018 · 2026-09-03 — Guid user ids; identity always from the token claim

- **Context:** The platform's domain entities identify users by `Guid` and read `User.FindFirstValue("UserID")` in controllers (e.g., `DepartmentsController`, `AdminLogsController`). The initial Auth code used an `int` identity key.
- **Decision:**
  - `User.Id` is now a **`Guid`** (`Id = Guid.NewGuid()` in the entity; `users.id` column is `uuid`). All user DTOs/commands/queries (`UserSummaryDto`, `GetCurrentUserQuery`, `SetUserActiveCommand`, …) take/return Guid.
  - The JWT stores the Guid in the `UserID` claim (plus standard `sub`/`nameidentifier`), and the **actor's identity is read from the token only** — never from a request body value. `ClaimsPrincipalExtensions.GetCurrentUserId()` (API project) is the single helper for that; controllers pass the resolved Guid into commands.
  - `PATCH /api/users/{id}/status` route takes a Guid.
- **Why:** Mirrors the WINSSAS convention; opaque Guids avoid enumeration/guessing of identity keys and keep actor identification trustworthy (client cannot claim to be someone else).
- **Consequences:** The `InitialAuth` migration was regenerated for the `uuid` column (dev DB only, re-seeded). Later domain entities (properties, clients, agreements, audit logs) should use `Guid` PKs too so `actor_user_id` FK types line up.

## D-019 · 2026-09-03 — Generated SQL scripts kept in `Scripts/` alongside EF migrations

- **Context:** Ops/DBAs and CI may need raw SQL rather than EF tooling; the source platform keeps SQL scripts in a top-level `Scripts/` folder.
- **Decision:** Mirror that convention: one generated, idempotent SQL file per EF migration under `Scripts/` (`0001_InitialAuth.sql` today), produced with `dotnet ef migrations script`. EF Core migrations in `RealEstateERP.Infrastructure` remain the source of truth; startup still applies them via `MigrateAsync`.
- **Why:** Cheap to generate, easy to review in PRs, and gives a runnable artifact for manual applies.
- **Consequences:** Workflow rule — every new `dotnet ef migrations add` ships with a matching numbered script in the same commit; regenerate instructions live in `Scripts/README.md`. Seed data is **not** in scripts (inserted by `DbSeeder` at startup).

## D-017 · 2026-09-03 — App projects use newer EF/Npgsql patch versions than the vendored library

- **Context:** Restore flagged **NU1903 (GHSA-x9vc-6hfv-hg8c)** — high-severity advisory on Npgsql 8.0.2, the version pinned by the source platform (and pulled transitively by `Infrastracture.Base.EF`).
- **Decision:** The new app projects reference **EF Core 8.0.11, Npgsql 8.0.9, Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11** directly. NuGet resolves these upward across the graph, so the vendored `Infrastracture.Base.EF` runs against the patched Npgsql too.
- **Consequences:** Keep app-level EF/Npgsql pins at the newest 8.0.x patch. Do not lower them to match the vendored csproj values; consider updating the vendored csproj versions in a later, separate change (D-003 treats that project as vendored).

## D-020 · 2026-09-04 — Audit trail: ChangeTracker-derived column diffs, written in-transaction

- **Context:** F-AUTH-04 (P0). The BUC mandates auditability (who did what, when); `security.md` requires an append-only trail for user admin, status changes, payments, document uploads/deletes, config/rate changes.
- **Decision:**
  - One general `audit_logs` table (migration `AddAuditLog`, [`Scripts/0002_AddAuditLog.sql`](../../Scripts/0002_AddAuditLog.sql)): `actor_user_id` (FK `users`, null for system actions), `action` (`user.created`, `user.deactivated`, `property.updated`, …), `entity_type`, `entity_id`, `details` (`jsonb`), `occurred_at`. Indexed on `(entity_type, entity_id, occurred_at)` and `(occurred_at)`.
  - `RealEstateDbContext` overrides `SaveChanges`/`SaveChangesAsync`: every Added/Modified/Deleted non-audit entity gets one row **in the same transaction**. No handler code writes audit rows — future modules get coverage for free.
  - **Details carry only the changed columns (old → new)**, derived from the ChangeTracker (`OriginalValues` vs `CurrentValues`); creates/deletes snapshot key identifying fields. Never full row snapshots.
  - Actor is the token's `UserID` claim resolved via `ICurrentUserService` (Infrastructure, `IHttpContextAccessor`); never a body value (D-018).
  - Noise control: `CreatedAt`/`UpdatedAt`/`LastLoginAt` are never audited (routine logins produce no rows); `PasswordHash` redacted to a `{"changed": true}` marker; `audit_logs` itself is never audited (no recursion).
- **Alternatives:** explicit `IAuditService.RecordAsync(...)` calls in every handler (precise action names, but easy to forget → gaps); full before/after row snapshots (rejected: ~10× table bloat at this scale).
- **Consequences:** Audit rows are append-only (no update/delete endpoints). If the table grows large, archive via month-partitioning rather than deleting (ERCA record-keeping). Dedicated history tables (`property_status_history`, `title_transfer_events`) remain the richer trail for critical workflows (D-010); `audit_logs` covers everything else.

## D-015 · 2026-09-03 — Documentation-first workflow for this repo

- **Context:** Living docs created for humans and AI assistants.
- **Decision:** `docs/status/*` is the source of truth for state; new architectural or stack decisions must be logged in `decisions.md` at the time they are made.
- **Consequences:** Anyone resuming work (or an AI) reads progress → decisions → feature-list before coding; see `docs/README.md`.
