# Database Schema

PostgreSQL 15+, EF Core 8 (Npgsql). Conventions:

- **Money** is `decimal(18,2)` and stored **in ETB on ledger tables**; an original-currency amount and the captured exchange rate are kept alongside for audit (see `payments`).
- **Dates** stored as UTC `timestamp` (the platform uses Npgsql legacy timestamp behavior off — prefer `timestamp with time zone`; keep `Npgsql.EnableLegacyTimestampBehavior` consistent with the imported code).
- **Primary keys**: `uuid` (`Guid`) — matches the domain-entity convention used across the `SES.WINSSAS` platform (their domain entities use `Guid Id`, DB default `gen_random_uuid()`). The ERP `User.Id` is a client-generated `Guid` (see `decisions.md` D-018).
- **Soft delete** where required (e.g., properties, clients): `is_deleted` + `deleted_at` (the imported repositories already treat deleted rows specially).
- Table/column names: `snake_case`. Entity → table mapping via EF `ToTable`/`HasColumnName`, or the `[Table]`/`[Column]` attributes already used by the source platform.
- Every mutable/status-changing table has a `created_at`, `created_by`, and `updated_at` for auditability; critical workflow tables additionally have a `_history`/event table.

## Domain entities (from the BUC "Data Entities Overview", expanded)

### users & roles

```
users
  id                uuid PK            -- Guid, client-generated (User.Id = Guid.NewGuid())
  full_name         varchar(200)
  email             varchar(200)  unique
  phone             varchar(30)
  username          varchar(100)  unique
  password_hash     varchar(255)          -- BCrypt.Net-Next
  role              varchar(30)           -- Admin | Sales | Finance | Legal | PropertyManager | ITAdmin
  is_active         boolean
  last_login_at     timestamptz
  created_at, updated_at

audit_logs             -- append-only trail (F-AUTH-04, D-020); written automatically on SaveChanges
  id                uuid PK
  actor_user_id     uuid FK -> users   -- from JWT UserID claim; null for system actions (seeding)
  action            varchar(100)   -- e.g. 'user.created', 'user.deactivated', 'title.transferred'
  entity_type       varchar(100)   -- CLR type name, e.g. 'User'
  entity_id         uuid
  details           jsonb          -- changed-column diff (old -> new) only; password hashes redacted
  occurred_at       timestamptz
  -- indexes: (entity_type, entity_id, occurred_at), (occurred_at)
```

### clients (buyers, sellers, tenants, landlords, Delalas)

```
clients
  id                uuid PK
  client_type       varchar(20)    -- Buyer | Seller | Tenant | Landlord | Delala  (a client may hold several: client_roles table)
  client_roles      (relation)     -- many-to-many client_types per client
  first_name, middle_name, last_name varchar(100)
  phone             varchar(30)
  email             varchar(200)
  id_type           varchar(30)    -- Passport | KebeleId | ...
  id_number         varchar(50)
  tin_number        varchar(50)    -- ERCA TIN
  is_company        boolean        -- corporate sellers/buyers
  company_name      varchar(200)
  is_deleted, deleted_at
  created_at, updated_at
  -- index: (tin_number), (phone), lower(last_name)

client_interactions
  id, client_id FK, staff_user_id FK, type varchar(30) -- Call|Meeting|SiteVisit
  notes text, occurred_at timestamptz

client_preferences
  id, client_id FK unique, budget_min/budget_max decimal(18,2),
  location_pref varchar(200), property_type_pref varchar(50)
```

### properties

```
properties
  id                uuid PK
  property_type     varchar(30)    -- Villa | Apartment | Condo | Commercial | Land
  title / description text
  location_sub_city varchar(100)   -- Addis Ababa sub-cities
  location_woreda   varchar(100)
  location_kebele   varchar(100)
  size_m2           decimal(10,2)
  bedrooms          int null
  bathrooms         int null
  amenities         jsonb          -- array of strings
  tenure_type       varchar(20)    -- Leasehold | Freehold
  lease_start_date  date null      -- leasehold
  lease_expiry_date date null      -- leasehold
  annual_ground_rent decimal(18,2) null  -- leasehold (ETB)
  status            varchar(30)    -- Available | UnderNegotiation | Reserved | Sold | Rented
                                   -- | LegalIssue | OnHold | OffMarket
  owner_client_id   uuid FK -> clients   -- current seller/landlord
  gov_assessed_value_etb decimal(18,2) null  -- Land Admin assessed value (used in tax calc)
  asking_price_etb  decimal(18,2) null
  currency          varchar(3) default 'ETB'  -- listing may be quoted USD
  is_deleted, deleted_at
  created_at, created_by, updated_at
  -- indexes: (status), (property_type, status), (location_sub_city), (owner_client_id)

property_documents      -- see documents table (polymorphic link by entity_type/entity_id)

property_status_history
  id, property_id FK, from_status, to_status, changed_by_user_id,
  reason text, changed_at timestamptz    -- full lifecycle audit (BUC NFR: auditability)

property_visits
  id, property_id FK, client_id FK, staff_user_id FK, scheduled_at timestamptz,
  outcome varchar(50), notes text
```

### agreements (sales & lease)

```
agreements
  id                uuid PK
  agreement_type    varchar(20)    -- Sales | Lease
  agreement_no      varchar(50) unique    -- human reference
  property_id       uuid FK -> properties
  seller_client_id  uuid FK -> clients null (lease: landlord)
  buyer_client_id   uuid FK -> clients null (lease: tenant)
  delala_client_id  uuid FK -> clients null  -- broker involved (1% commission)
  currency          varchar(3) default 'ETB'
  exchange_rate_to_etb decimal(18,6) null     -- captured NBE rate if non-ETB
  agreed_amount     decimal(18,2)
  amount_etb        decimal(18,2) computed/stored
  down_payment_pct  decimal(5,2)              -- >= 10% business rule
  down_payment_etb  decimal(18,2)
  terms             text
  language          varchar(10)               -- am | en (agreement language)
  status            varchar(30)   -- Draft | PendingSignatures | Active | InTransfer |
                                  -- Completed | Cancelled | OnHold | LegalIssue
  signed_at / title_transferred_at timestamptz null
  created_at, created_by, updated_at
  -- indexes: (property_id), (buyer_client_id), (status), (agreement_no)

agreement_documents      -- generated + scanned agreements (link to documents)

payment_schedules       -- installment plan (installments)
  id, agreement_id FK, installment_no int, due_date date,
  amount decimal(18,2), amount_etb decimal(18,2),
  status varchar(20)     -- Pending | Paid | Overdue | Waived
  -- index: (due_date, status) for overdue reminders

title_transfers          -- UC-RE-001 core state
  id, agreement_id FK unique,
  status varchar(30)     -- Initiated | TitleVerification | DocumentsPrepared |
                         -- TaxesPaid | TransferCompleted | Rejected
  legal_officer_user_id uuid FK -> users
  verification_result   text
  encumbrance_checked   boolean
  title_deed_document_id uuid FK -> documents   -- new title deed (የባለቤትነት ማረጋገጫ)
  checklist jsonb
  completed_at timestamptz
  created_at, updated_at

title_transfer_events    -- history/audit of each step (who, when, note)
  id, title_transfer_id FK, from_status, to_status,
  changed_by_user_id uuid FK, note text, occurred_at timestamptz
```

### payments & finance

```
payments
  id                uuid PK
  payment_type      varchar(30)   -- DownPayment | Installment | Rent | StampDuty |
                                  -- TransferTax | CapitalGainsTax | Commission | Deposit
  agreement_id      uuid FK -> agreements null   -- sales/lease payments
  rent_bill_id      uuid FK -> rent_bills null
  client_id         uuid FK -> clients           -- payer
  amount            decimal(18,2)
  currency          varchar(3)
  exchange_rate_to_etb decimal(18,6)
  amount_etb        decimal(18,2)
  method            varchar(30)   -- Cash | BankTransfer | Cheque | EPayment
  reference_number  varchar(100)  -- bank/transfer reference
  received_date     date
  recorded_by_user_id uuid FK -> users
  status            varchar(20)   -- Recorded | Bounced | Refunded
  notes             text
  created_at, updated_at
  -- indexes: (agreement_id), (client_id), (received_date), (status)

commissions
  id, agreement_id FK, recipient_type varchar(20)  -- Agency | Delala
  recipient_client_id uuid FK -> clients null    -- Delala client
  percentage decimal(5,2), amount decimal(18,2), amount_etb decimal(18,2)
  status varchar(20)     -- PendingApproval | Approved | Paid | Disputed
  approved_by_user_id uuid FK, paid_at timestamptz
  created_at, updated_at
  -- commission voucher / signed record referenced from agreements (Risk R9 mitigation)

tax_calculations       -- snapshot at calculation time; rates are configurable (Risk R6)
  id, agreement_id FK,
  tax_type varchar(30)    -- CapitalGains | StampDuty | TransferTax
  base_amount decimal(18,2)         -- profit (seller) or max(assessed, price)
  rate decimal(7,4), amount decimal(18,2), amount_etb decimal(18,2)
  payable_by varchar(10)            -- Buyer | Seller
  paid_payment_id uuid FK -> payments null
  calculated_at, calculated_by_user_id
  -- unique index: (agreement_id, tax_type)

general_ledger_entries
  id, entry_date date, account_code varchar(30),
  description text, debit decimal(18,2), credit decimal(18,2),
  payment_id uuid FK -> payments null, agreement_id uuid FK null
  posted_by_user_id uuid FK, posted_at timestamptz
  -- accounts: SalesRevenue | RentalIncome | CommissionIncome | TaxPayable | BankAccounts | Cash

exchange_rates         -- daily NBE rates
  id, currency_code varchar(3), rate_date date,
  rate_to_etb decimal(18,6),
  source varchar(50),    -- Manual | NBEImport
  created_at
  -- unique index: (currency_code, rate_date)
```

### rental & maintenance

```
leases (extends agreements where agreement_type='Lease' via lease fields + lease table)
rent_bills
  id, lease_id FK (agreement_id), period_month date, rent_amount decimal(18,2),
  due_date date, late_fee decimal(18,2) null,
  status varchar(20)      -- Open | PartiallyPaid | Paid | Overdue
  -- index: (due_date, status)

maintenance_requests
  id, property_id FK, tenant_client_id FK, description text,
  reported_date date, priority varchar(20),
  assigned_user_id uuid FK null,
  status varchar(30)      -- Reported | Assigned | InProgress | Resolved | Closed
  resolution_notes text, resolved_at timestamptz
  -- linked work-order lifecycle

eviction_processes
  id, lease_id FK, status varchar(30) -- NoticeIssued | InProgress | Completed
  legal_notice_document_id uuid FK -> documents, notes text
```

### documents (shared)

```
documents
  id                uuid PK
  document_type     varchar(40)   -- TitleDeed | SalesAgreement | LeaseAgreement |
                                  -- Receipt | TaxForm | IDCopy | CommissionVoucher | ...
  entity_type       varchar(30)   -- Property | Agreement | Client | TitleTransfer
  entity_id         uuid
  object_key        varchar(300)  -- MinIO object name (DocumentIdentifier-based)
  bucket            varchar(100)
  file_name         varchar(255)
  extension         varchar(10)
  content_type      varchar(100)
  size_bytes        bigint
  sha256            varchar(64)   -- integrity check (Risk R1 mitigation)
  uploaded_by_user_id uuid FK, uploaded_at timestamptz
  is_deleted, deleted_at
  -- indexes: (entity_type, entity_id), (document_type), (uploaded_at)
```

> Document bytes live in **MinIO**; the `documents` table is metadata only. Upload/download goes through `IDocumentService` (`MinioDocumentService`), matching the existing `DocumentBase`/`DocumentRequestBase` types.

## Notable relationships (FK summary)

- `agreements.property_id → properties.id`; `agreements.{seller,buyer,delala}_client_id → clients.id`
- `payments.agreement_id → agreements.id`; `commissions.agreement_id → agreements.id`
- `tax_calculations.agreement_id → agreements.id`
- `title_transfers.agreement_id → agreements.id (1:1)`; `title_transfer_events.title_transfer_id → title_transfers.id`
- `payment_schedules.agreement_id → agreements.id`
- `property_status_history.property_id → properties.id`; `audit_logs` cover the rest

## Seed data

- Default roles + admin user (`Admin` role).
- Currency codes (`ETB`, `USD`, ...) and current `exchange_rates` snapshot.
- Default configurable rates in `app_settings`/`tax_rate_settings` table mirroring the BUC: CGT 15%, stamp duty 2%, transfer tax 2%, agency commission 2%, Delala commission 1%, min down payment 10%.
