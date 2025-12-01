# ADR 0002: Add Testing Reset/Seed Endpoints

## Status
Accepted

## Context
Playwright E2E tests and backend integration tests were intermittently failing because analytics data was stateful. We needed a repeatable way to clear and seed surveys while ensuring production security posture remains unchanged.

## Decision
Expose `/testing/reset` and `/testing/seed` endpoints only when `ASPNETCORE_ENVIRONMENT=Testing`. These endpoints are guarded by the `X-Test-Auth` header, reuse the same FluentValidation/InputSanitizer logic as production, and back tests with the in-memory EF Core provider.

## Consequences
- ✅ Playwright and integration suites can deterministically prepare data via HTTP.
- ✅ No code paths are exposed when running in Development or Production.
- ⚠️ CI/CD pipelines must set `Testing__ApiKey`/`TESTING_API_KEY` consistently.
- ⚠️ Additional maintenance is required to keep helper endpoints aligned with production request contracts.
