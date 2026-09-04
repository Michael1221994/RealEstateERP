# Database scripts

Generated SQL for the `realestate_erp` PostgreSQL schema. **EF Core migrations are the source of truth** (`src/RealEstateERP.Infrastructure/Migrations`); these scripts are generated artifacts for manual/ops applies, CI, and DBAs who don't run EF tooling.

## Files

| File | EF migration | Contents |
| --- | --- | --- |
| `0001_InitialAuth.sql` | `InitialAuth` | `users` table (Guid/`uuid` PK), unique username index |
| `0002_AddAuditLog.sql` | `AddAuditLog` | `audit_logs` table (append-only trail: actor, action, entity, `jsonb` diff), FKs + indexes |

## Applying

```bash
# Fresh database
psql -U RealEstateERP -h localhost -p 5433 -d realestate_erp -f Scripts/0001_InitialAuth.sql
```

Run the files in numeric order on a fresh DB. Each is guarded (`IF NOT EXISTS`) and transactional, so re-runs are harmless. Note: the app also applies migrations automatically on startup (`MigrateAsync`); the seed data (initial `admin`) is inserted by `DbSeeder` at startup, not by these scripts.

## Keeping it up to date (as you go)

After adding a new EF migration with `dotnet ef migrations add <Name>`, generate its SQL script here:

```bash
# Delta for the new migration only (FROM the previous one):
dotnet ef migrations script <PreviousMigration> <NewMigration> --idempotent \
  --project src/RealEstateERP.Infrastructure \
  --startup-project src/RealEstateERP.API \
  --output Scripts/<NNNN>_<NewMigration>.sql

# ...or the whole schema from scratch (replaces what a fresh apply needs):
dotnet ef migrations script --idempotent \
  --project src/RealEstateERP.Infrastructure \
  --startup-project src/RealEstateERP.API \
  --output Scripts/<NNNN>_<NewMigration>.sql
```

Name files with a zero-padded sequence number (`0002_…`, `0003_…`) so ordering stays obvious. Commit the script in the same change as the migration.
