# Testing Strategy

Framework: **xUnit** (`tests/RealEstateERP.Tests`), the same choice as the source platform's test projects (which use `Microsoft.EntityFrameworkCore.InMemory`, `Microsoft.Extensions.Logging`, xunit).

## Test pyramid

| Level | What | Where |
| --- | --- | --- |
| Unit | calculators (tax/commission/FX), state-transition maps, DTO mapping, enum rules, pure helpers | `tests/…/Unit/` |
| Handler | MediatR handlers against **in-memory fakes** of repositories / `IRepository` (or EF InMemory), asserting `Response<T>` outcomes incl. `NotFound`/`Error` | `tests/…/Features/<Feature>/` |
| Integration | Full endpoint flows against the real API composition: HTTP + EF InMemory or a disposable **Testcontainers** PostgreSQL | `tests/…/Integration/` |
| (Later) E2E | few happy paths incl. MinIO stub & PDF rendering | opt-in, slow |

### Notes on DB choice

- Default for handler/repo tests: **EF InMemory** (matches the source platform's test projects) or SQLite in-memory where relational behavior matters.
- Add **Testcontainers.PostgreSql** tests for migration + raw-SQL-sensitive queries (e.g., `IRepository.ExecuteQuery`) — the generic repository runs real SQL there.
- A **seed factory** (`TestDataFactory`) builds valid fixtures: properties, clients (incl. Delala), agreements, payment schedules, rates. Keep factories in `tests/…/Common/`.

## What to test first (priority by business risk)

1. **F-SALES-02 tax/commission calculations** — table-driven tests with the BUC figures (15% CGT on profit; 2% + 2% on max(assessed, price); 2% agency; 1% Delala; USD conversion with captured rate).
2. **State machines** — property status and title-transfer transition maps; illegal transitions return `Error`; role enforcement (only Legal → `TitleTransferred`).
3. **Money/FX** — decimal precision, rounding, ETB snapshot correctness (Risk TR-6).
4. **Auth** — login, wrong password, expired/invalid token, role claims enforced (403).

## Conventions

- File per tested type: `<TypeUnderTest>Tests.cs` (e.g., `TaxCalculatorTests.cs`).
- Arrange–Act–Assert with blank-line separation; name tests `Method_Scenario_ExpectedResult`:
  `CalculateStampDuty_AssessedAbovePrice_UsesAssessedValue`.
- Prefer `Theory`/`InlineData` for table cases; keep one behavior per test.
- Assertions on the **envelope**: check `responseStatus`, then `data`; use `FluentAssertions` if introduced (log decision) — otherwise xUnit asserts.
- Async everywhere (`Task`-returning tests).

## Integration-test setup (once API exists)

- `WebApplicationFactory<Program>` host; override config (`ConnectionStrings`, MinIO endpoint → stub/MinIO container).
- In-memory or container DB per test class; clear/seed between tests.
- Auth helper: issue a JWT for a given role (`Sales`, `Finance`, `Legal`, `Admin`) so tests exercise `[Authorize(Roles=…)]`.

## Coverage targets

- **Core handlers/services: ≥ 80%** line coverage (business rules live here).
- Calculators/state machines: **100%** of rule branches is the norm.
- API controllers: smoke-level (happy path + unauthorized per endpoint) — controllers are thin.
- Repositories: coverage via integration tests of the queries they back; no strict % target.
- CI gate: failing build, failing tests, or < 70% overall on Core blocks merge (once CI exists).

## What we do NOT test

- Vendored `Infrastructure/` internals (MinIO client details, iText rendering) — test only the seams we call (`IDocumentService` against a stub, PDF returns non-empty for Amharic sample).
- Visual/UI (no UI in Phase 1).

## Reporting

- `dotnet test` runs all; `--filter` by area for quick loops.
- Coverage: `dotnet test --collect:"XPlat Code Coverage"` + report via `coverlet` (already a dependency pattern in the source platform's test csproj).
