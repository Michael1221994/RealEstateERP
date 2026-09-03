# Technical Risks

Adapted from the requirements' Risk Register, focused on **technical** risks for this backend. Tracked with ID, likelihood/impact, and mitigation. Revisit at each milestone.

Legend — Likelihood / Impact: **H** high · **M** medium · **L** low.

| ID | Risk | L / I | Status | Mitigation |
| --- | --- | --- | --- | --- |
| **TR-1** | Schema churn & data loss during early migrations (agreements/payments tables evolving fast) | H / H | Open | Migration-per-feature discipline (`EF migrations add` per feature), never edit applied migrations, backup DB before `database update`, seeders idempotent. Schema change procedure in `setup.md`. |
| **TR-2** | Tax-rate/regulation changes invalidate stored calculations (Risk R6) | M / H | Mitigated (D-011) | Configurable rates + per-row snapshots in `tax_calculations`/`commissions`; changelog of config changes; ERCA export regenerable from snapshots. |
| **TR-3** | Title-deed forgery / tampered scans (Risk R1) | M / H | Mitigated (D-008) | Store `sha256` per document; uploader + timestamp audit; Legal verification step gates transfer; original deed scans kept immutable (no overwrite, new version rows). |
| **TR-4** | Performance: 10,000+ properties, 50 concurrent users, <2 s tax calc (BRD NFR) | M / M | Open | Indexes from `database-schema.md`; pagination on all lists (`IRepository`); no N+1 (use `GetQueryAsync` + `Include`); tax/commission calc is in-memory on one aggregate — fast. Load-test at M6. |
| **TR-5** | N+1 queries & lazy-loading surprises through the generic repository | M / M | Open | Repositories return shaped queries with eager `Include`; handlers never trigger lazy loads after the query (serialization config); code-review checklist item; tests assert query counts where cheap. |
| **TR-6** | Money precision / FX rounding bugs (USD↔ETB, Risk R2) | M / H | Mitigated (D-007) | `decimal` everywhere; store original + rate + ETB snapshot; rounding strategy (round-half-up at 2 dp) documented & unit-tested; GL posts ETB only. |
| **TR-7** | Amharic text broken in generated PDFs/Excel (missing font, shaping) | M / M | Mitigated (D-012) | Use bundled `nyala.ttf` via the imported iText generator; render test with real Amharic strings before feature sign-off. |
| **TR-8** | MinIO unavailability / document loss (Risk R4 subset) | M / H | Open | Health check on MinIO; retry on upload; document metadata recoverable; backups include bucket (off-site per Risk R4); object key derived from `DocumentIdentifier` (existing `DocumentBase` contract). |
| **TR-9** | Dependency on vendored `Infrastracture.*` library (upstream bugs / stale versions) | M / M | Open | Treated as vendored code (`D-003`); keep a diff/change log if patched; upgrade in lockstep with the source platform when available; tests cover the integration seams we rely on. |
| **TR-10** | JWT key compromise / weak secrets (Risk R4 subset) | M / H | Open | Secrets via env/user-secrets only; strong key length; rotation documented; rate limiting + security headers middleware from Base.API enabled; audit of failed logins (later). |
| **TR-11** | ERCA export format changes | M / M | Open | Export logic isolated behind a report service; Excel-first (per BRD) using ClosedXML; formats versioned in code with tests against fixture files. |
| **TR-12** | Data inconsistency between payments and property/agreement status (e.g., marked Sold before full payment) | M / H | Open | State machine in Core (D-010): `TitleTransferred`/`Sold` transitions require completed prerequisites; reconciliation query reports mismatches; GL posting synchronous with payment recording. |
| **TR-13** | Late-discovery requirement gaps (Delala disputes, deficiency notices, eviction) | M / M | Open | Feature template forces acceptance criteria + BUC references before build; stakeholder review of `feature-list.md` at M3. |
| **TR-14** | Offline-mode expectation (Risk R8) conflicts with online-only Phase 1 | M / L | Accepted (D-014) | Explicitly out of scope; process-level mitigation for site visits (paper + later entry); revisited at Phase 2. |
| **TR-15** | Single-developer bus factor / knowledge concentrated in docs | M / M | Open | This living-doc tree + AI-readable navigation (docs/README.md); pair code review on the first two features to lock conventions. |
| **TR-16** | Cost basis ambiguity blocks capital gains accuracy (D-013) | M / H | Open | Resolve D-013 with stakeholders before F-FIN-03; provisional CGT estimate design already keeps it non-blocking for sales flow. |

## How to update this file

- New risk → add row, assign next `TR-n`, set status Open.
- Risk addressed by a decision → link `D-0nn` and set status **Mitigated** (keep the row — context value).
- Review the whole register at each milestone (M3, M4, M6, M8).
