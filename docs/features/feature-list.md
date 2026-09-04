# Feature List

Every feature maps to a module (see [`../architecture/module-breakdown.md`](../architecture/module-breakdown.md)) and, where relevant, to BUC UC-RE-001 steps. Track progress by ticking the boxes. Status legend: ⬜ Not Started · 🔶 In Progress · ✅ Complete.

Priorities: **P0** = needed for Phase 1 go-live core · **P1** = important · **P2** = nice-to-have / later phase.

---

## Foundation

- [x] **F-INF-01 — Scaffold solution & wire references** · P0 · ✅ (2026-09-03)
  `RealEstateERP.sln` + Core/Infrastructure/API/Tests referencing the imported libraries — see [`setup.md`](../project/setup.md).
- [x] **F-INF-02 — Global error handling + logging baseline** · P0 · ✅ (2026-09-03)
  `GlobalExceptionHandlingMiddleware`, security headers + rate-limit middleware wired in `Program.cs`; structured `ILogger` usage.
- [ ] **F-INF-03 — Seed data & initial migration** · P0 · 🔶 (initial migration + admin seed done; full lookup/rates seed pending)
  Roles, admin user, currency codes, configurable tax/commission rates, sample lookup values.

## Auth & Users (module: Auth)

- [x] **F-AUTH-01 — Login & JWT issuance** · P0 · ✅ (2026-09-03)
- [x] **F-AUTH-02 — Role-based access control policies** · P0 · ✅ (2026-09-03)
- [ ] **F-AUTH-03 — User CRUD & activation** · P1 · 🔶 (create/list/activate done; edit & delete pending)
- [x] **F-AUTH-04 — Audit log capture** · P0 · ✅ (2026-09-04)
- [ ] **F-AUTH-05 — Password change/reset** · P1 · ⬜

## Property (module: Property)

- [ ] **F-PROP-01 — Property CRUD** · P0 · ⬜
- [ ] **F-PROP-02 — Property status lifecycle with history** · P0 · ⬜ (BUC steps 11, extensions 3a/4a/6a)
- [ ] **F-PROP-03 — Property documents (title deed, cadastral map)** · P0 · ⬜
- [ ] **F-PROP-04 — Property search/filter + pagination** · P0 · ⬜
- [ ] **F-PROP-05 — Listing flyer generation (Amharic/English PDF)** · P2 · ⬜
- [ ] **F-PROP-06 — Visits & site-visit scheduling** · P1 · ⬜

## Client (module: Client)

- [ ] **F-CLI-01 — Client CRUD (buyer/seller/tenant/landlord/Delala)** · P0 · ⬜
- [ ] **F-CLI-02 — Client search (name/TIN/phone) + type filter** · P0 · ⬜
- [ ] **F-CLI-03 — Client interactions log** · P1 · ⬜
- [ ] **F-CLI-04 — Client preferences** · P2 · ⬜
- [ ] **F-CLI-05 — Per-client transaction history view** · P1 · ⬜

## Sales (module: Sales — BUC UC-RE-001)

- [ ] **F-SALES-01 — Create sales agreement** · P0 · ⬜ (happy path step 1)
- [ ] **F-SALES-02 — Auto-calculate taxes & commissions** · P0 · ⬜ (step 2; rules in BUC)
- [ ] **F-SALES-03 — Agreement generation (bilingual PDF, pre-filled)** · P0 · ⬜ (step 3)
- [ ] **F-SALES-04 — Signature capture / scanned copy upload** · P0 · ⬜ (step 4)
- [ ] **F-SALES-05 — Down payment recording & payment schedule** · P0 · ⬜ (step 5)
- [ ] **F-SALES-06 — Title verification workflow** · P0 · ⬜ (step 6; Legal-only)
- [ ] **F-SALES-07 — Transfer document checklist + deficiency notices** · P1 · ⬜ (steps 7, 7a)
- [ ] **F-SALES-08 — Stamp duty & transfer tax payment tracking** · P0 · ⬜ (step 8)
- [ ] **F-SALES-09 — Title transfer completion + new title deed upload** · P0 · ⬜ (steps 9, 10)
- [ ] **F-SALES-10 — Property mark-Sold + commission release** · P0 · ⬜ (step 11)
- [ ] **F-SALES-11 — Post-sale documents (confirmations, receipts, statements)** · P1 · ⬜ (step 12)
- [ ] **F-SALES-12 — Negotiation/on-hold/legal-issue flags** · P1 · ⬜ (extensions 3a, 4a, 6a)
- [ ] **F-SALES-13 — Bounced payment handling** · P1 · ⬜ (extension 8a)
- [ ] **F-SALES-14 — Delala commission dispute log** · P1 · ⬜ (extension 10a)

## Rental (module: Rental)

- [ ] **F-RENT-01 — Lease agreement creation** · P0 · ⬜
- [ ] **F-RENT-02 — Rent bills, partial payments, receipts** · P0 · ⬜
- [ ] **F-RENT-03 — Overdue tracking & reminders** · P1 · ⬜
- [ ] **F-RENT-04 — Maintenance request lifecycle** · P1 · ⬜
- [ ] **F-RENT-05 — Eviction process (legal notice generation)** · P2 · ⬜

## Finance & Accounting (module: Finance)

- [ ] **F-FIN-01 — Payment recording (multi-currency, ETB conversion)** · P0 · ⬜
- [ ] **F-FIN-02 — Exchange-rate maintenance (manual + NBE import)** · P0 · ⬜
- [ ] **F-FIN-03 — Capital gains calculation (seller cost basis)** · P0 · ⬜ (D-013 dependency)
- [ ] **F-FIN-04 — Tax payment vouchers (ERCA)** · P0 · ⬜
- [ ] **F-FIN-05 — Commission approval & payment** · P0 · ⬜ (Risk R9)
- [ ] **F-FIN-06 — General ledger auto-posting** · P1 · ⬜
- [ ] **F-FIN-07 — Bank reconciliation** · P1 · ⬜
- [ ] **F-FIN-08 — Configurable tax/commission rates + changelog** · P0 · ⬜ (Risk R6)

## Document Management (module: Documents)

- [ ] **F-DOC-01 — Upload/download documents (MinIO + metadata)** · P0 · ⬜
- [ ] **F-DOC-02 — Document metadata + integrity hash** · P0 · ⬜ (Risk R1)
- [ ] **F-DOC-03 — Role-restricted document access** · P0 · ⬜
- [ ] **F-DOC-04 — Document audit trail** · P0 · ⬜

## Reporting (module: Reporting)

- [ ] **F-RPT-01 — Monthly sales report (Excel/PDF)** · P0 · ⬜
- [ ] **F-RPT-02 — Rental occupancy & collection report** · P1 · ⬜
- [ ] **F-RPT-03 — Tax liability summary (ERCA export)** · P0 · ⬜
- [ ] **F-RPT-04 — Profit & loss statement** · P1 · ⬜
- [ ] **F-RPT-05 — Delala performance report** · P1 · ⬜

---

## Phase 2 (parked, not yet broken into tasks)

Client mobile app · banking API auto-confirmation · GIS property mapping · full Amharic UI + Ethiopian calendar display · offline mode with sync.
