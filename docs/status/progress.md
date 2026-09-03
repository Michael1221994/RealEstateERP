# Progress

> **Overall: ~15% — Phase 1 (backend) foundation + Auth core delivered.**
> Last updated: **2026-09-03**.

## Milestones

| # | Milestone | Status | Date |
| --- | --- | --- | --- |
| M0 | BUC (UC-RE-001) + BRD requirements document | ✅ Complete | 2026-09-03 |
| M1 | Shared Infrastructure (`Infrastracture.Base*`) imported into repo and verified to build | ✅ Complete | 2026-09-03 |
| M2 | Living documentation created | ✅ Complete | 2026-09-03 |
| M3 | Solution scaffolded (`RealEstateERP.sln`, Core/Infrastructure/API/Tests, refs, initial migration) | ✅ Complete | 2026-09-03 |
| M4 | Auth core: login + JWT + role authorization + admin seeding | ✅ Complete (audit log F-AUTH-04 still open) | 2026-09-03 |
| M5 | Property & Client modules (foundation CRUD) | ⬜ Not started | — |
| M6 | Sales module incl. UC-RE-001 title-transfer workflow | ⬜ Not started | — |
| M7 | Rental module | ⬜ Not started | — |
| M8 | Finance & accounting (payments, commissions, taxes, GL) | ⬜ Not started | — |
| M9 | Document management (MinIO upload/retrieval) | ⬜ Not started | — |
| M10 | Reporting (Excel/PDF, ERCA exports) | ⬜ Not started | — |

## Completed

- **2026-09-03** — BUC `UC-RE-001` (Property Sales and Title Transfer) and BRD reviewed; tax/commission rules, statuses, and entity overview extracted into the docs.
- **2026-09-03** — `Infrastracture.Base`, `Infrastracture.Base.API`, `Infrastracture.Base.EF` (plus `SES.WINSSAS.Common`) copied into this repo at their original relative layout; all three projects **build with 0 errors** under .NET SDK 9.0.315.
- **2026-09-03** — Full living-documentation tree under `docs/`.
- **2026-09-03** — Solution scaffolded: `RealEstateERP.sln` with `src/RealEstateERP.Core`, `src/RealEstateERP.Infrastructure`, `src/RealEstateERP.API`, `tests/RealEstateERP.Tests` (all net8.0). Build: **0 warnings / 0 errors**. Tests: **9/9 passing**.
- **2026-09-03** — Auth module (F-AUTH-01/02/03 core): `users` table + initial EF migration `InitialAuth`; bcrypt password hashing; `POST /api/auth/login` issuing signed JWTs (role + user claims); default-deny auth with `[Authorize(Roles)]` (`Admin`/`ITAdmin` on user management); `/api/auth/me`; create/list/activate users; admin seeded on startup from `Seed:*` config.
- **2026-09-03** — User identity switched to **Guid** (WINSSAS convention, see [`decisions.md`](decisions.md) D-018): `users.id` is `uuid`, `InitialAuth` regenerated + re-applied; the JWT `UserID` claim carries the Guid and `ClaimsPrincipalExtensions.GetCurrentUserId()` (API) is the single claim→actor helper (identity never taken from request bodies); routes take `{id:guid}`. Verified live against local Docker Postgres (`postgres-erp`, `localhost:5433`): login, `/api/auth/me`, role-guarded `/api/users`, and activate/deactivate-by-Guid all resolve the actor from the token. Build 0 errors; tests **9/9**.

## In progress

- **F-AUTH-04 audit log capture** (rows in `audit_logs`) — next small auth task.
- Next coding milestone after that: Property + Client modules (M5 / F-PROP-01, F-CLI-01).

## Blocked

- **Nothing blocked.** One decision to confirm with stakeholders before deep design of finance features: whether cost basis for capital-gains "profit" is tracked per seller per property (recommended) or approximated (see [`decisions.md`](decisions.md) D-013).

## Next steps

1. Add audit-log capture (F-AUTH-04) while auth is fresh.
2. Implement Property + Client CRUD (M5) end-to-end as the reference feature for the whole codebase (see `feature-template.md` example).
3. Then the Sales module and the UC-RE-001 title-transfer workflow (see `backlog.md`).
