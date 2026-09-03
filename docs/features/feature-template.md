# Feature Template

Copy this template for each feature. File name: `<feature-id>.md` (e.g., `F-SALES-02.md`) placed in a per-feature folder under `docs/features/` once the feature moves to 🔶 In Progress — or keep single-file tracking in `feature-list.md` until then.

---

## Feature: {Feature Name}

- **ID:** {e.g., F-SALES-02}
- **Module:** {Sales}
- **Priority:** P0 / P1 / P2
- **Status:** ⬜ Not Started / 🔶 In Progress / ✅ Complete
- **BUC reference:** {UC-RE-001 step n, extension n, or BRD section}
- **Owner / Assigned to:** {optional}

### Description

{2–5 sentences. What does this feature do for whom, and why. Ground in the BUC/BRD.}

### Acceptance criteria

```gherkin
Scenario: ...
  Given ...
  When ...
  Then ...
```

or a numbered checklist:

- [ ] {criterion 1}
- [ ] {criterion 2}

### API endpoints involved

| Method | Path | Role | Notes |
| --- | --- | --- | --- |
| {POST} | {/api/...} | {Finance} | {request/response shape → api-design.md} |

### Database changes

- {table/column changes, indexes, FKs → database-schema.md}
- {migration name}

### Business rules

- {rule 1 — e.g., "Capital gains tax 15% on profit, payable by Seller"}
- {rule 2}

### Out of scope / non-goals

- {explicitly excluded so scope stays clear}

### Testing notes

- {unit: calculator/handler cases}
- {integration: endpoint scenarios}
- {edge cases}

### Dependencies

- {other features/modules this relies on}
- {blocked by …}

---

## Example (filled): F-SALES-02 — Auto-calculate taxes & commissions

- **ID:** F-SALES-02
- **Module:** Sales
- **Priority:** P0
- **Status:** ⬜ Not Started
- **BUC reference:** UC-RE-001 happy path step 2; Business Rules section.

### Description

When a Sales Agent records an agreed sale (price + terms) on a property, the system automatically computes the taxes payable (capital gains, stamp duty, transfer tax) and commissions (agency, Delala) per the business rules, snapshots the rates used, and stores the breakdown for later voucher/posting use.

### Acceptance criteria

- [ ] Creating a sales agreement returns the tax + commission breakdown with `agreementId` (see `api-design.md` response example).
- [ ] Stamp duty = 2% and transfer tax = 2% of **max(government-assessed value, agreed price)** — payable by Buyer.
- [ ] Capital gains tax = 15% of **profit** (agreed price − seller cost basis) — payable by Seller; surfaced as *estimate* until Finance finalizes cost basis.
- [ ] Agency commission = 2% of sale price; Delala commission = 1% when a Delala client is attached to the agreement.
- [ ] Amounts use decimal; non-ETB agreements convert via the day's captured NBE rate and record it.
- [ ] Rates come from configuration (`TaxRates:*` / `Commission:*`), never hard-coded.
- [ ] Calculation completes in < 2 s (NFR).

### API endpoints involved

| Method | Path | Role | Notes |
| --- | --- | --- | --- |
| POST | `/api/sales-agreements` | Sales | returns breakdown in response `data` |
| GET | `/api/sales-agreements/{id}/tax-breakdown` | Finance, Legal | read the snapshot |

### Database changes

- `tax_calculations` table (snapshot: `tax_type`, `base_amount`, `rate`, `amount`, `payable_by`, `agreement_id`).
- `commissions` table (rows for Agency + optional Delala, `status=PendingApproval`).
- Migration: `AddTaxAndCommissionSnapshots`.

### Business rules

- Taxes on **max(government-assessed value, agreed price)** — the assessed value lives on `properties.gov_assessed_value_etb`.
- Capital gains 15% on seller's **profit** ⇒ needs cost basis (see D-013).
- Delala commission only "if involved" (Delala client present on the agreement).
- Commission payments require later approval (F-FIN-05, Risk R9) — calculation itself only records.

### Out of scope / non-goals

- Actually collecting/posting the money (F-SALES-08, F-FIN-01).
- Recalculation after rate changes — new snapshots are created, old ones are never mutated (audit).

### Testing notes

- Unit: `TaxCalculator`/`CommissionCalculator` with rate edge cases (zero price, assessed > agreed, USD quote with captured rate, no Delala).
- Unit: transition map — agreement `Draft → PendingSignatures` only when breakdown computed.
- Integration: POST `/api/sales-agreements` returns 200 envelope with expected figures (fixtures from BUC numbers).
- Property-based: money never negative; decimal precision.

### Dependencies

- F-PROP-01 (property + assessed value), F-CLI-01 (clients), F-FIN-03 (cost basis for CGT), F-INF-03 (seeded rates).
