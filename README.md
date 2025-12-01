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

## API Endpoints

- `POST /api/survey` - Submit a survey response
- `GET /api/survey/nps` - Get current NPS score
- `GET /api/survey/average` - Get average rating
- `GET /api/survey/distribution` - Get rating distribution
- `POST /testing/reset` (Testing env only, `X-Test-Auth` required) - Clear all surveys
- `POST /testing/seed` (Testing env only, `X-Test-Auth` required) - Seed surveys for deterministic tests

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