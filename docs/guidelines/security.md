# Security Guidelines

Applies to the API. Existing building blocks come from the imported libraries (`Infrastracture.Base.API`): global exception middleware, security-headers middleware, rate limiting, role/workflow authorization handlers, `InAppIdentityAuthorizedController`.

## Authentication

- **JWT bearer** (local issuer, D-009). Passwords hashed with **BCrypt.Net-Next** (same library as the source platform).
- Secrets never in `appsettings.json` commits — use environment variables / user-secrets (`Jwt:SecretKey`, DB passwords).
- Token validation: issuer, audience, lifetime, signing key. Default-deny policy (every endpoint authenticated unless `[AllowAnonymous]` — login only).
- *(Optional upgrade path)* multi-realm Keycloak: the `Program.cs` dynamic-scheme pattern from `SES.WINSSAS.Entitlement.API` is the reference; enable via `Jwt:Schemes`.

## Authorization (RBAC)

- Roles: `Admin, Sales, Finance, Legal, PropertyManager, ITAdmin`.
- Enforce at two levels:
  1. **Endpoint**: `[Authorize(Roles = "...")]` (or reuse `InAppIdentityAuthorizedController` where the requester profile is needed).
  2. **Workflow (domain)**: state-transition guards in Core handlers — e.g., only `Legal` may move `title_transfers.status` to `TransferCompleted`; only `Finance` records payments; commissions require `Finance/Admin` approval before `Paid`.
- Default-deny for new endpoints; never default to `AllowAnonymous`.
- Delalas are **clients**, not system users — no login (D-009).

## Input validation

- Validate every request in the handler layer (commands carry DTOs, never raw entities).
- Rules: required fields, ranges (percentages 0–100, money ≥ 0), enum membership, date sanity (due dates after agreement date), TIN/phone formats where known.
- Use FluentValidation only if adopted repo-wide (log the decision); until then validation lives in handlers and is unit-tested.
- ASP.NET `[ApiController]` automatic model-state handling stays on.

## Common-vulnerability protections

| Threat | Protection |
| --- | --- |
| **SQL injection** | EF Core parameterization everywhere. Raw SQL only via `IRepository.ExecuteQuery(query, parameters)` **with parameters**, never string-concatenated input. Review any new raw SQL. |
| **XSS** | API returns JSON only; `ResponseSecurityHeadersMiddleware` (imported) sets headers; never reflect user HTML. Report/PDF text is rendered through templates (iText), not HTML passthrough. |
| **CSRF** | Bearer-token API, no cookie auth for the API — CSRF not applicable to token flows; keep `SameSite` sane for any future cookie-based UI session. |
| **Broken auth / token theft** | Short-lived tokens; HTTPS in all environments except localhost; security headers; audit login failures. |
| **Mass assignment** | Commands pick explicit fields; controllers never bind entities from the body. |
| **IDOR** | Ownership checks in handlers (entity belongs to caller's org/client where applicable) — even within an intranet staff system, Finance vs Sales data is separated by role + handler checks. |
| **DoS** | Rate limiting middleware (imported) on auth endpoints; pagination caps (`pageSize` max); no unbounded queries. |

## Data protection

- **In transit:** TLS everywhere outside local dev; ASP.NET `UseHttpsRedirection` enabled on non-dev profiles.
- **At rest:**
  - PostgreSQL: disk encryption per host policy; backups encrypted (Risk R4).
  - Secrets: env vars / user-secrets / a secret manager; **never** plaintext in the repo (the source repo's `.env`/docker-compose style is a cautionary example — do not copy credentials into committed files).
  - Sensitive personal data (IDs, TINs, phones): access restricted to Finance/Legal/Admin surfaces; export logs reviewed.
  - Documents in MinIO: bucket private; server-side access via `IDocumentService` with role checks; store `sha256` for integrity (TR-3).
- **Passwords/tokens:** never logged; never returned in responses.

## Audit

- `audit_logs` records actor, action, entity, timestamp for: title-transfer steps, status changes, document uploads/deletes, payment records, config/rate changes, user admin actions.
- Trace id on every request; error responses carry it; logs carry it (correlate).
- Audit rows are append-only (no update/delete endpoints).

## Operational security

- Health checks (imported `HealthCheck`) for DB + MinIO.
- Hangfire dashboard (if enabled) restricted to `Admin` and not exposed publicly in production.
- Dependency/package updates tracked; vendor review applies to `Infrastructure/` changes (D-003).
- Before any production deploy: review checklist = secrets externalized · HTTPS enforced · rate limits on · security headers on · default-deny auth on · audit enabled · backups verified.
