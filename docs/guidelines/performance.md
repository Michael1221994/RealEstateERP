# Performance Guidelines

Targets from the BRD: **10,000+ properties**, **50 concurrent users**, tax/commission calculations **< 2 s**, 99.5% availability.

## 1. Database indexing (see `database-schema.md` for full list)

Hot queries and their indexes:

| Query pattern | Index |
| --- | --- |
| Property lists filtered by status/type | `properties(status)`, `properties(property_type, status)` |
| Property search by location | `properties(location_sub_city, location_woreda)` |
| Client search by TIN/phone | `clients(tin_number)`, `clients(phone)` |
| Agreement lookup by reference | `agreements(agreement_no)` unique |
| Overdue installments / rent bills (reminders) | `payment_schedules(due_date, status)`, `rent_bills(due_date, status)` |
| Payments reconciliation by date range | `payments(received_date)` |
| Audit queries by entity | `audit_logs(entity_type, entity_id, occurred_at)` |
| Exchange rate daily lookup | `exchange_rates(currency_code, rate_date)` unique |

Rules: index what the paginated WHERE/ORDER/JOIN actually use; don't over-index write-heavy tables; run `EXPLAIN ANALYZE` on new query-heavy features.

## 2. Query discipline (avoid N+1)

- Use `IRepository.GetQueryAsync<T>()` + eager `.Include(...)` / `.ThenInclude(...)` inside repositories/handlers; materialize once.
- **No lazy loading** after the request handler returns (serialization then can't trigger loads). Disable lazy loading in the DbContext configuration.
- Project to DTOs for list screens (`Select`), not full entities with children.
- Raw SQL (`ExecuteQuery`) reserved for genuinely awkward aggregations (reports), always parameterized.
- Budget: 10k properties × list endpoint must stay < ~300 ms p95 (check during load test at M6).

## 3. Pagination & list limits

- All list endpoints paginate: `pageIndex`, `pageSize` (cap e.g. 200), optional `sortBy`/`sortOrder` — the `IRepository.GetAsync` pagination contract already provides this.
- Count queries cached/cheap: use `IRepository.CountAsync` with the same filter.
- Reports are not paginated user lists — they go through async generation instead (below).

## 4. Caching strategy

- **Phase 1:** in-memory `IMemoryCache` (already registered in the DI pattern) for:
  - daily exchange rates (invalidated on new import),
  - lookup/seed data (property types, sub-cities, tax-rate config),
  - current config rates (short TTL, invalidated on config change).
- **Phase 2 (if needed):** Redis (`IDistributedCache`) when multiple instances or longer-lived cache is required — log a D-decision before introducing.
- Never cache money balances or statuses without an explicit invalidation path; audit writes bypass cache.

## 5. Async processing for heavy work

Use **Hangfire** (already available in `Infrastracture.Base.API`) for:
- overdue payment reminders (nightly job),
- report generation for large ranges (generate → store → notify),
- NBE exchange-rate daily import,
- PDF batch generation (agreements/flyers) if a user triggers many at once.

Interactive paths (create agreement, record payment) stay synchronous — they're small and must return the computed envelope immediately (`< 2 s` rule).

## 6. Other

- JSON: reference handling & string enums configured once in `Program.cs` (as in the source APIs); don't serialize navigation graphs accidentally (`ReferenceHandler.Preserve` + DTOs).
- Logging: avoid `Information` logs in hot read paths (see `coding-standards.md`).
- Connection pooling: default Npgsql pooling; keep DbContext scoped; never hold DbContexts statically.
- Health checks include a DB ping with a timeout so MinIO/DB outages fail fast rather than hanging requests (pattern: timeout config on HTTP clients, as in the source platform).
- Load test before milestone sign-off (M6, M8): 10k seeded properties, 50 concurrent users on list + agreement-create + payment-record; record p95 numbers here after the run.
