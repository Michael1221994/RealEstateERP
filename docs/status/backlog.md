# Backlog

Ordered by priority for **Phase 1 go-live**. Effort is in rough dev-days (S/M/L). Cross-reference IDs in [`../features/feature-list.md`](../features/feature-list.md).

Legend: **S** ≈ ≤2 days · **M** ≈ 2–5 days · **L** ≈ 5–10 days.

| # | Item | Priority | Effort | Depends on | Notes |
| --- | --- | --- | --- | --- | --- |
| 1 | F-INF-01 Scaffold solution + project references | P0 | S | — | M3 milestone; do first |
| 2 | F-INF-03 Seed data & initial migration | P0 | M | 1 | roles, admin, currencies, rate config |
| 3 | F-INF-02 Global error handling + logging baseline | P0 | S | 1 | register Base.API middleware |
| 4 | F-AUTH-01 Login & JWT issuance | P0 | M | 1 | bcrypt hashing, `Jwt` config |
| 5 | F-AUTH-02 Role-based authorization policies | P0 | M | 4 | default-deny + `[Authorize(Roles)]` |
| 6 | F-AUTH-04 Audit log capture | P0 | M | 1 | write-through on mutations |
| 7 | F-PROP-01 Property CRUD | P0 | M | 2, 5 | reference feature for pattern |
| 8 | F-PROP-03 Property documents | P0 | M | 7, F-DOC-01 | |
| 9 | F-PROP-02 Property status lifecycle + history | P0 | M | 7 | transition map + history table |
| 10 | F-PROP-04 Property search/filter + pagination | P0 | M | 7 | `IRepository` pagination |
| 11 | F-CLI-01 Client CRUD (incl. Delala role) | P0 | M | 2, 5 | |
| 12 | F-CLI-02 Client search | P0 | S | 11 | TIN/phone/name indexes |
| 13 | F-DOC-01 Upload/download documents (MinIO) | P0 | M | 1 | `IDocumentService` |
| 14 | F-DOC-02 Metadata + sha256 | P0 | S | 13 | Risk R1 |
| 15 | F-DOC-03/04 Access control + audit trail | P0 | M | 13, 6 | |
| 16 | F-SALES-01 Create sales agreement | P0 | M | 7, 11 | |
| 17 | F-SALES-02 Auto-calculate taxes & commissions | P0 | M | 16 | see example in `feature-template.md` |
| 18 | F-SALES-03 Bilingual agreement PDF generation | P0 | L | 16, F-DOC-01 | iText + `nyala.ttf` |
| 19 | F-SALES-05 Down payment & payment schedule | P0 | M | 16, F-FIN-01 | |
| 20 | F-FIN-01 Payment recording (multi-currency) | P0 | M | 11 | |
| 21 | F-FIN-02 Exchange-rate maintenance | P0 | M | 20 | manual first, NBE import later |
| 22 | F-SALES-06 Title verification workflow | P0 | L | 16, 6 | Legal-only transitions |
| 23 | F-SALES-08 Stamp duty & transfer tax payment tracking | P0 | M | 17, 20 | |
| 24 | F-FIN-03 Capital gains (cost basis) | P0 | M | 16, 20 | **decision D-013 required** |
| 25 | F-SALES-09 Title transfer completion + deed upload | P0 | M | 22, 13 | |
| 26 | F-SALES-10 Mark-Sold + commission release | P0 | M | 25, 23, F-FIN-05 | |
| 27 | F-FIN-04 Tax vouchers (ERCA) | P0 | M | 23, 18 | |
| 28 | F-FIN-05 Commission approval & payment | P0 | M | 26, 20 | Risk R9 |
| 29 | F-FIN-08 Configurable tax rates + changelog | P0 | S | 2 | Risk R6 |
| 30 | F-RPT-01 Monthly sales report (Excel/PDF) | P0 | L | 26 | ClosedXML / iText |
| 31 | F-RPT-03 Tax liability summary (ERCA export) | P0 | M | 27 | |
| 32 | F-SALES-04 Signature capture / scanned copy | P1 | M | 16, 13 | |
| 33 | F-SALES-07 Transfer checklist + deficiency notices | P1 | M | 22 | |
| 34 | F-SALES-11 Post-sale documents | P1 | M | 26 | |
| 35 | F-SALES-12 On-hold / legal-issue flags | P1 | M | 9 | extensions 3a/4a/6a |
| 36 | F-SALES-13 Bounced payment handling | P1 | S | 20 | extension 8a |
| 37 | F-SALES-14 Delala dispute log | P1 | S | 28 | extension 10a |
| 38 | F-CLI-03 Interactions log | P1 | S | 11 | |
| 39 | F-CLI-05 Transaction history view | P1 | M | 11, 20 | |
| 40 | F-PROP-06 Visits | P1 | M | 11 | |
| 41 | F-AUTH-03 User CRUD | P1 | M | 4 | |
| 42 | F-AUTH-05 Password reset | P1 | S | 4 | |
| 43 | F-RENT-01 Lease agreement | P1 | M | 7, 11 | Phase 1 core |
| 44 | F-RENT-02 Rent bills + partial payments | P1 | M | 43, 20 | |
| 45 | F-RENT-03 Overdue tracking & reminders | P1 | M | 44 | Hangfire |
| 46 | F-RENT-04 Maintenance lifecycle | P1 | M | 43 | |
| 47 | F-FIN-06 GL auto-posting | P1 | L | 20, 43 | |
| 48 | F-FIN-07 Bank reconciliation | P1 | L | 20 | |
| 49 | F-RPT-02 Occupancy & collection | P1 | M | 43 | |
| 50 | F-RPT-04 Profit & loss | P1 | M | 47 | |
| 51 | F-RPT-05 Delala performance | P1 | M | 28 | |
| 52 | F-RENT-05 Eviction process | P2 | L | 43 | legal notice PDF |
| 53 | F-PROP-05 Listing flyers | P2 | M | 18 | |
| 54 | F-CLI-04 Client preferences | P2 | S | 11 | |

**Phase 2 backlog (unestimated):** mobile app · banking API auto-confirmation · GIS mapping · Amharic UI + Ethiopian calendar display · offline mode + sync.
