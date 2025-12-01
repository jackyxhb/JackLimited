# JackLimited

A full-stack web application for collecting and analyzing customer feedback through Net Promoter Score (NPS) surveys, complete with deterministic testing utilities for CI/CD.

## Features

- Submit customer feedback surveys with ratings and comments
- Real-time NPS calculation and analytics (NPS, averages, distributions)
- Testing-only reset/seed endpoints secured by an API key for deterministic suites
- Responsive Vue 3 frontend with dark/light theme toggle
- ASP.NET Core Minimal API backend with shared sanitization/validation pipeline
- PostgreSQL database for production, in-memory provider for development/testing
- CORS enabled for cross-origin requests

## Tech Stack

### Frontend
- Vue 3 with TypeScript
- Vite for build tooling
- Pinia for state management
- Vue Router for navigation
- Playwright for end-to-end testing
- Vitest for unit testing

### Backend
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL (production) / In-Memory DB (development + testing)
- FluentValidation + shared InputSanitizer for consistent validation/sanitization
- Test-only reset/seed endpoints guarded by `X-Test-Auth`

## Prerequisites

- .NET 8.0 or later
- Node.js 18+ and npm
- PostgreSQL (for production)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/jackyxhb/JackLimited.git
   cd JackLimited
   ```

2. Set up the backend:
   ```bash
   cd src/backend/JackLimited.Api
   dotnet restore
   ```

3. Set up the frontend:
   ```bash
   cd ../../../frontend
   npm install
   ```

## Configuration

### Backend Configuration

Update `appsettings.json` or `appsettings.Development.json` for database connection:

```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=jacklimited;Username=youruser;Password=yourpassword"
   }
}
```

For the testing utilities, set an API key (defaults to `local-testing-key`) that must match the inbound header:

```json
{
   "Testing": {
      "ApiKey": "super-secret-test-key"
   }
}
```

When the backend runs with `ASPNETCORE_ENVIRONMENT=Testing`, deterministic helpers become available:

- `POST /testing/reset` clears all survey data.
- `POST /testing/seed` accepts an array of `SurveyRequest` objects to pre-populate data.

Both endpoints require the `X-Test-Auth` header whose value must equal `Testing:ApiKey`.

### Frontend Configuration

The frontend proxies API traffic to `/api` and Playwright injects `TESTING_API_KEY` so E2E tests can talk to the backend helpers. Set `TESTING_API_KEY` in your shell (for CI) if you override the backend key.

## Running the Application

### Development

1. Start the backend:
   ```bash
   cd src/backend/JackLimited.Api
   dotnet run
   ```

2. In a new terminal, start the frontend:
   ```bash
   cd frontend
   npm run dev
   ```

3. Open your browser to `http://localhost:5173`.

> Tip: When iterating on E2E tests locally you can reuse the dev servers above; Playwright will detect and reuse them.

### Production Build

1. Build the backend:
   ```bash
   cd src/backend/JackLimited.Api
   dotnet publish -c Release
   ```

2. Build the frontend:
   ```bash
   cd frontend
   npm run build
   ```

## Testing

### Backend Tests
```bash
cd src/backend/JackLimited.Tests
dotnet test
```

The integration suite automatically runs the API in the `Testing` environment and calls the `/testing/reset` helper before each test with the `X-Test-Auth` header.

### Frontend Unit Tests
```bash
cd frontend
npm run test:unit
```

### End-to-End Tests (Playwright)
Playwright boots both the backend (Testing mode) and the frontend for you:

```bash
cd frontend
TESTING_API_KEY=super-secret-test-key npm run test:e2e
```

- `TESTING_API_KEY` must match `Testing__ApiKey` supplied to the backend (`local-testing-key` by default).
- Tests call `/testing/reset` and `/testing/seed` to guarantee deterministic analytics data before performing assertions.

### API Documentation

Swagger/OpenAPI docs are available whenever the backend runs in Development or Testing mode:

```
http://localhost:5264/swagger
```

Use the explorer to try requests, inspect schemas, or export the OpenAPI JSON (`/swagger/v1/swagger.json`).

### Docker Container

You can run the full stack (published backend plus built frontend assets) via Docker:

```bash
docker build -t jacklimited .
docker run -p 8080:8080 jacklimited
```

The container uses Ubuntu-based .NET 8.0 images and listens on `http://localhost:8080`, serving both the API and the compiled SPA.

## API Endpoints

- `POST /api/survey` - Submit a survey response
- `GET /api/survey/nps` - Get current NPS score
- `GET /api/survey/average` - Get average rating
- `GET /api/survey/distribution` - Get rating distribution
- `POST /testing/reset` (Testing env only, `X-Test-Auth` required) - Clear all surveys
- `POST /testing/seed` (Testing env only, `X-Test-Auth` required) - Seed surveys for deterministic tests

## Versioning

- The repository root `VERSION` file is the single source of truth for the semantic version (format `MAJOR.MINOR.PATCH`).
- GitHub Actions reads that value to tag Docker images (`jackyxhb/jacklimited:<version>` plus the commit SHA).
- After a successful push of those images on `main`, the workflow automatically bumps the patch number, commits the change as `chore: bump version to X.Y.Z [skip ci]`, and pushes it back.
- To request a larger jump, include `[bump minor]` or `[bump major]` in the triggering commit message; the workflow resets lower-order segments accordingly.
- Manual edits to `VERSION` are still possible (for example before cutting a release branch), but avoid combining them with the automated bump in the same push.

## CI/CD

Automated validation runs through GitHub Actions (`.github/workflows/ci.yml`) on every push and pull request targeting `main`. The workflow:

- Builds and tests all .NET 8.0 projects.
- Installs Node 22, lints, runs Vitest unit tests, and builds the frontend.
- Builds the Docker image (using Ubuntu-based .NET 8.0 images) to ensure containerization stays healthy.
- Scans the Docker image for HIGH and CRITICAL vulnerabilities using Trivy; fails the build if any are found.
- On pushes to `main`, logs into Docker Hub (using `DOCKERHUB_USERNAME`/`DOCKERHUB_TOKEN` repo secrets) and pushes `jackyxhb/jacklimited` tagged with the commit SHA and the semantic version from `VERSION`.
- Creates a Git tag (e.g., `v1.0.5`) on the successful commit.
- Once the pushes succeed, the workflow bumps `VERSION` (patch by default) and commits the change with `[skip ci]` so the automation does not loop.

## Architecture Decision Records

ADR files documenting key choices live in `docs/adr/`. Start with:

- `0001-monorepo-structure.md` – explains why the frontend remains at the repo root.
- `0002-deterministic-testing-endpoints.md` – captures the rationale behind the `/testing/*` helpers.

## Project Structure

```
JackLimited/
├── frontend/                 # Vue.js frontend
├── src/backend/              # .NET backend
│   ├── JackLimited.Api/      # Main API project
│   ├── JackLimited.Application/  # Application logic
│   ├── JackLimited.Domain/   # Domain models
│   ├── JackLimited.Infrastructure/  # Data access
│   └── JackLimited.Tests/    # Unit tests
├── JackLimited.sln           # Solution file
└── README.md                 # This file
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.