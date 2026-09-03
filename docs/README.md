# Project Documentation

Living documentation for the **Ethiopian Real Estate Agency ERP** backend.

## For an AI assistant resuming work

Follow this order to get oriented fast:

1. **`/docs/project/readme.md`** — what the project is, where it stands, and pointers to source documents.
2. **`/docs/status/progress.md`** — current state: what is done, what is in progress, what is blocked, next steps.
3. **`/docs/status/decisions.md`** — the technical decisions already made (read before proposing alternatives).
4. **`/docs/features/feature-list.md`** — the full backlog of features and their status.
5. **`/docs/architecture/system-overview.md`** — how the code is laid out and how the layers talk to each other.
6. **`/docs/architecture/`** (schema, api-design, module-breakdown) + **`/docs/guidelines/`** — details only when you need them for a specific task.

Rule of thumb: **never propose a stack, layer, or pattern that contradicts `/docs/status/decisions.md` without first updating that file with the new decision and its rationale.**

## Folder purpose

| Folder | Purpose |
| --- | --- |
| `project/` | What this is, how to run it, how to read the docs. |
| `architecture/` | System design: high-level layout, DB schema, API contracts, module breakdown. |
| `features/` | Feature list (the work breakdown) and a template for describing one feature. |
| `status/` | Where the project stands: progress, backlog, decision log, technical risks. |
| `guidelines/` | How we write code: standards, testing, security, performance. |

## Status badge

> **Phase 1 (backend) — foundation + Auth core delivered (2026-09-03).** Solution scaffolded; login/JWT/RBAC working and tested. Next: audit log, Property & Client modules.
