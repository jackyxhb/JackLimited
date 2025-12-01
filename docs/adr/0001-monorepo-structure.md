# ADR 0001: Preserve Top-Level Frontend Directory

## Status
Accepted

## Context
The repository houses both a Vue/Vite frontend and a .NET backend. Historically, the frontend has lived at the repository root (`frontend/`) while backend projects are under `src/backend/`. Some contributors proposed moving the frontend beneath `src/` for symmetry.

## Decision
Keep the frontend at the repository root. This keeps toolchains decoupled, avoids path churn in existing scripts/docs, and highlights the polyglot nature of the project when browsing the repo.

## Consequences
- ✅ No changes required for existing npm/vite/playwright commands or documentation.
- ✅ Easier mental model for developers who frequently `cd frontend` and `cd src/backend/...`.
- ⚠️ Contributors should remain aware that the `src/` directory currently houses backend-only code.
