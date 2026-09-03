# API Design

REST over JSON, ASP.NET Core controllers. Base path: `/api`.

## Conventions

### Response envelope

Every endpoint returns the shared `Response<T>` envelope (`Infrastracture.Base`):

```json
{
  "responseStatus": "Success",
  "systemMessage": null,
  "isFailed": false,
  "message": "Property created",
  "messageCode": null,
  "data": { "...": "..." }
}
```

`responseStatus` ∈ `Success | Error | Warning | Info | NotFound`. `isFailed == true` when status is `Error` or `data` is null.

### Error format

Errors are sanitized by the global exception middleware (`GlobalExceptionHandlingMiddleware`, registered first in `Program.cs`). Clients see a safe message; full detail + trace id is logged server-side (`SafeError.TraceId` pattern):

```json
{
  "responseStatus": "Error",
  "message": "An unexpected error occurred. Reference: 7f3a...",
  "messageCode": "7f3a...",
  "data": null
}
```

Validation failures return `Error` with a readable `message` (or a `Warning`/`NotFound` per case). HTTP status follows semantics: 200 with envelope for handled domain outcomes (per existing platform style), 401/403 from the auth middleware before a handler runs.

### Authentication & authorization

- **Default-deny**: every endpoint requires a valid bearer JWT unless marked `[AllowAnonymous]` (login).
- **Roles** (claim `role`): `Admin`, `Sales`, `Finance`, `Legal`, `PropertyManager`, `ITAdmin`.
- Endpoint access via `[Authorize(Roles = "...")]`. Controllers that need the requester resolve the actor with `ClaimsPrincipalExtensions.GetCurrentUserId()` (API) — the identity is read from the token's `UserID` claim only, never from the request body (see [`decisions.md`](../status/decisions.md) D-018) — and pass it into commands as `createdBy`/actor context.
- **Workflow guards**: state transitions like "mark title transferred" additionally require role **and** a legal-officer check (see UC-RE-001 step 10a / NFR *"Only Legal Officer can change status to Title Transferred"*).

### Query/pagination

List endpoints accept `?pageIndex=0&pageSize=20&sortBy=...&sortOrder=Ascending` (the `IRepository.GetAsync` pagination contract) and may add filters (`status`, `type`, `locationSubCity`, `q` for search).

---

## Auth

### POST /api/auth/login  *(AllowAnonymous)*
Authenticates and returns a signed JWT.

Request:
```json
{ "username": "hanna.finance", "password": "********" }
```
Response `data`:
```json
{
  "token": "eyJhbGciOi...",
  "expiresAt": "2026-09-03T17:00:00Z",
  "user": { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "fullName": "Hanna Bekele", "role": "Finance" }
}
```

## Properties

| Method | Path | Role | Purpose |
| --- | --- | --- | --- |
| GET | `/api/properties` | Sales, Admin, PropertyManager | list (filters + pagination) |
| GET | `/api/properties/{id}` | authenticated | detail incl. documents, history |
| POST | `/api/properties` | Sales, Admin, PropertyManager | create listing |
| PUT | `/api/properties/{id}` | Sales, Admin, PropertyManager | update listing |
| PATCH | `/api/properties/{id}/status` | Sales, Admin, PropertyManager | status change (`Available`, `Reserved`, `UnderNegotiation`, `OffMarket`, ...) |
| POST | `/api/properties/{id}/documents` | Sales, Admin | attach title deed / cadastral scan |
| GET | `/api/properties/{id}/status-history` | authenticated | full lifecycle audit |

Create request example:
```json
{
  "propertyType": "Apartment",
  "locationSubCity": "Bole",
  "locationWoreda": "03",
  "locationKebele": "01",
  "sizeM2": 120,
  "bedrooms": 3,
  "tenureType": "Leasehold",
  "leaseExpiryDate": "2056-06-30",
  "annualGroundRent": 2400,
  "askingPrice": 8500000,
  "currency": "ETB",
  "ownerClientId": 12
}
```

## Clients

| Method | Path | Role | Purpose |
| --- | --- | --- | --- |
| GET | `/api/clients` | Sales, Admin | search (name/TIN/phone), filter by type |
| GET | `/api/clients/{id}` | Sales, Finance, Admin | detail + transaction history |
| POST | `/api/clients` | Sales, Admin | register buyer/seller/tenant/landlord/Delala |
| PUT | `/api/clients/{id}` | Sales, Admin | update profile/TIN/ID |
| POST | `/api/clients/{id}/interactions` | Sales | log call/meeting/site visit |

Create request (Delala example):
```json
{
  "firstName": "Girma", "lastName": "Alemu", "phone": "+251911123456",
  "idType": "KebeleId", "idNumber": "01-2345-6789", "tinNumber": "0012345678",
  "roles": ["Delala", "Seller"]
}
```

## Sales agreements (incl. UC-RE-001)

| Method | Path | Role | Purpose |
| --- | --- | --- | --- |
| POST | `/api/sales-agreements` | Sales | create agreement → computes taxes + commissions |
| GET | `/api/sales-agreements/{id}` | Sales, Finance, Legal, Admin | detail |
| POST | `/api/sales-agreements/{id}/generate-agreement` | Sales | generate bilingual PDF agreement (Amharic/English) |
| POST | `/api/sales-agreements/{id}/sign` | Sales, Legal | record signatures / upload scanned signed copy |
| POST | `/api/sales-agreements/{id}/down-payment` | Finance | record down payment (cash/bank/cheque) |
| POST | `/api/sales-agreements/{id}/title-transfer/start` | Legal | initiate title verification |
| POST | `/api/sales-agreements/{id}/title-transfer/verify` | Legal | record verification + encumbrance result |
| POST | `/api/sales-agreements/{id}/title-transfer/complete` | Legal | **mark Title Transferred** (legal-only) |
| POST | `/api/sales-agreements/{id}/finalize` | Finance | record final payment + capital gains voucher |
| POST | `/api/sales-agreements/{id}/commissions/release` | Finance, Admin | release agency/Delala commissions |
| GET | `/api/sales-agreements/{id}/tax-breakdown` | Finance, Legal | taxes: capital gains, stamp duty, transfer tax |

Create request example:
```json
{
  "propertyId": 45,
  "sellerClientId": 12,
  "buyerClientId": 88,
  "delalaClientId": 90,
  "currency": "ETB",
  "agreedAmount": 8500000,
  "downPaymentPct": 20,
  "installments": [ { "dueDate": "2026-11-30", "amount": 1700000 } ],
  "language": "am"
}
```

Response `data` includes the auto-calculated summary (illustrative numbers):
```json
{
  "agreementId": 1001,
  "agreementNo": "SA-2026-0001",
  "status": "Draft",
  "priceEtb": 8500000,
  "downPaymentEtb": 1700000,
  "taxes": {
    "stampDuty": 170000,
    "transferTax": 170000
  },
  "commissions": { "agencyEtb": 170000, "delalaEtb": 85000 }
}
```
*(Capital gains tax is finalized later by Finance because it depends on the seller's cost basis.)*

## Rentals

| Method | Path | Role |
| --- | --- | --- |
| POST | `/api/leases` | PropertyManager, Admin |
| GET | `/api/leases/{id}` | PropertyManager, Finance |
| POST | `/api/leases/{id}/rent-bills/generate` | PropertyManager |
| POST | `/api/rent-bills/{id}/pay` | Finance |
| GET | `/api/properties/{id}/maintenance-requests` | PropertyManager, Tenant-facing staff |
| POST | `/api/properties/{id}/maintenance-requests` | PropertyManager |
| PATCH | `/api/maintenance-requests/{id}/status` | PropertyManager |

## Payments, commissions, GL

| Method | Path | Role | Purpose |
| --- | --- | --- | --- |
| GET | `/api/payments?agreementId=&clientId=&from=&to=` | Finance, Admin | list/reconcile |
| POST | `/api/payments` | Finance | record payment (any type) |
| POST | `/api/payments/{id}/bounce` | Finance | bounced cheque (extension 8a) |
| GET | `/api/commissions` | Finance, Admin, Sales | commission register |
| POST | `/api/commissions/{id}/approve` | Finance, Admin | approval before payment (Risk R9) |
| GET | `/api/gl/entries` | Finance, Admin | ledger entries |
| POST | `/api/reconciliations/bank` | Finance | bank reconciliation import/compare |

## Documents

| Method | Path | Role |
| --- | --- | --- |
| POST | `/api/documents` (multipart) | authenticated (role per doc type) |
| GET | `/api/documents/{id}/download` | authenticated |
| GET | `/api/documents?entityType=&entityId=` | authenticated |
| DELETE | `/api/documents/{id}` | Admin, Legal (title deeds) |

## Reports

| Method | Path | Role |
| --- | --- | --- |
| GET | `/api/reports/monthly-sales?month=&year=&format=excel\|pdf` | Admin, Sales Manager, Finance |
| GET | `/api/reports/rental-occupancy` | Admin, PropertyManager |
| GET | `/api/reports/tax-summary?year=` | Finance, Admin (ERCA export) |
| GET | `/api/reports/profit-loss?from=&to=` | Admin, Finance |
| GET | `/api/reports/delala-performance` | Admin, Sales Manager |

Reports support `lang=am|en`; PDFs render Amharic via the bundled font (iText + `nyala.ttf`).

## API design rules (enforced in review)

1. Controllers never contain business logic — build a command/query, call `_mediator.Send(...)`.
2. Never return a raw entity when a DTO exists; entities expose navigation properties.
3. Mutations that change domain state must record who (`createdBy`/actor from claims) — auditability NFR.
4. Money always `decimal`, never `float`; DTOs include `currency` where non-ETB is legal.
5. State transitions are validated in Core handlers (enum + allowed-transition map), not in controllers.
