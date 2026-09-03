# Coding Standards

Applies to new code in this repo. The vendored `Infrastructure/` libraries follow their own (inherited) style — leave them alone (see `decisions.md` D-003).

## Naming conventions

| Thing | Convention | Example |
| --- | --- | --- |
| Projects | `RealEstateERP.<Layer>` / `RealEstateERP.<Module>` | `RealEstateERP.Infrastructure` |
| Namespaces | `RealEstateERP.<Layer>.<Folder>` | `RealEstateERP.Core.Features.Sales.Contract.Command` |
| Types (classes/interfaces/enums) | PascalCase; interfaces prefix `I` | `ISalesAgreementRepository` |
| Public members, methods | PascalCase, `Async` suffix | `Task<Response<T>> CreateAsync(...)` |
| Private/local | `camelCase`; private fields `_camelCase` | `_mediator` |
| Constants | PascalCase (`PascalCase` for `const`) | `MaxInstallments` |
| Method names | verbs | `CalculateTaxes`, `MarkSold` |
| Files | match the public type, one type per file | `SalesAgreement.cs` |
| Folders | feature-first (see layout rule) | `Features/Sales/Contract/Command/` |
| DB tables/columns | `snake_case` | `title_transfers`, `gov_assessed_value_etb` |
| HTTP routes | kebab-case nouns, plural resources | `/api/sales-agreements` |
| JSON property names | camelCase (default ASP.NET Core) | `agreementNo` |

## Folder structure rules

- **Features live in Core under `Features/<FeatureName>/`** with the sub-structure:
  ```
  Contract/Command/  Contract/Query/  Contract/Service/  Contract/Repository/
  Handler/Command/   Handler/Query/   Handler/Service/
  DTOs/
  ```
  This mirrors the source platform and keeps a use case's contracts, handlers, and DTOs together.
- **Entities** that are shared domain concepts (no single feature owner) go in `Core/Models/`; enums in `Core/Enums/` (or next to the entity).
- **Controllers** live in `API/Controllers/`, one per resource, thin (see API rules in `api-design.md`).
- **Per-feature repository interfaces** belong in Core (`Contract/Repository`); **EF implementations** in `Infrastructure/Repository/`.
- **DI registration**: one static extension `AddInfrastructure(this IServiceCollection, IConfiguration)` in `Infrastructure/Dependency/DependencyInjection.cs`. Register interfaces from Core → implementations from Infrastructure; shared services from the imported libraries (MinIO, PDF, repository) are registered there too, per the `SES.WINSSAS` example.
- Do not create folders named `Helpers`, `Utils`, `Common` in app code for one-off functions — put the code next to its feature. (Tolerate them in vendored code.)

## Language & typing rules

- `nullable` enabled, `ImplicitUsings` enabled (as in the imported projects).
- **Money = `decimal`**, never `float`/`double`. Money DTOs include `currency`.
- Dates: store UTC `DateTime`/`DateTimeOffset`; convert at boundaries; keep Npgsql legacy timestamp behavior **consistent with the imported code** (set the same switch in `Program.cs`).
- Prefer `async`/`await` throughout; no `.Result`/`.Wait()`.
- Prefer records for DTOs where helpful, classes for entities.

## Error handling

- **Handlers/services return `Response<T>`** — do not throw for expected domain outcomes (invalid state, not found). Use `Response<T>.NotFound(...)` / `.Error(...)`.
- **Unexpected exceptions** propagate to `GlobalExceptionHandlingMiddleware` (registered first in `Program.cs`). It returns a sanitized message with a trace id; log full detail at the handler with `ILogger`.
- Never expose exception strings, SQL, or stack traces to clients.
- Validate inputs in handlers (or FluentValidation validators if introduced — log the decision if adopted); never trust controller-bound entities from the body.

## Logging standards

- Use `ILogger<T>` injected; **no `Console.WriteLine`** in app code.
- Log context: method scope, entity ids, actor id where relevant. Example: `_logger.LogError(ex, "Error reading agreement {AgreementId}", id);`
- Log at: `Debug` (diagnostics), `Information` (state changes: created/signed/transferred), `Warning` (bounced payments, retries), `Error` (unexpected failures). Avoid logging at `Information` in hot read paths.
- Never log passwords, tokens, TINs in full, or document contents.
- Include a correlation/trace id on requests (middleware) and echo it in error responses.

## Code review checklist

- [ ] No business logic in controllers; handlers return `Response<T>`.
- [ ] State transitions validated in Core; actor (`createdBy`) captured from claims for mutations.
- [ ] Money uses `decimal` + currency handling; no float.
- [ ] N+1 avoided — eager includes inside repository/handler queries, no lazy-load surprises.
- [ ] Sensitive data not logged; no secrets in code or committed config.
- [ ] Feature uses existing shared contracts (`IRepository`, `IDocumentService`, …) instead of new ad-hoc abstractions.
- [ ] DB change has a migration + updated `database-schema.md`; rates/config are not hard-coded.
- [ ] BUC/feature reference present (`feature-list.md` IDs); acceptance criteria testable.
- [ ] Vendored `Infrastructure/` files untouched unless the change is D-logged.
- [ ] Tests added per `testing-strategy.md`.

## Inherited quirks to accept

- `Infrastracture.*` (typo) and `Extentions` (typo) are real namespaces/folders in the vendored libraries — spell them **exactly** when referencing.
- The imported projects occasionally use uppercase folders like `DTOs`/`DTOS`; new code uses `DTOs` consistently.
