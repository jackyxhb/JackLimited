# JACK LIMITED FEEDBACK PORTAL – COPILOT DEVELOPMENT GUIDE (2025)

You are working on **Jack Limited Feedback Portal** – a production-grade web application with:
- Backend: ASP.NET Core 9.0 Minimal APIs
- Frontend: Vue 3 + TypeScript + Vite + Pinia + Chart.js
- Database: PostgreSQL + EF Core 9
- Architecture: Vertical Slice + Clean Architecture principles
- Testing: 100% TDD + BDD (xUnit + SpecFlow or bUnit for Blazor if added later)
- Deployment: Azure App Service (primary) / AWS Elastic Beanstalk (secondary)

## CORE PHILOSOPHY – RED → GREEN → REFACTOR (TDD) + GIVEN-WHEN-THEN (BDD)

Every single feature, bug fix, or refactoring MUST follow this cycle:

1. **Write a failing test first** (red)
2. **Make it pass with minimal code** (green)
3. **Refactor safely** (Copilot will suggest cleanups – always accept only if all tests still pass)

→ **Never write production code without a failing test first**  
→ **Never merge a PR with less than 90% test coverage on new code**

## PROJECT STRUCTURE (DO NOT DEVIATE – Copilot will enforce this)

src/
├── backend/
│   ├── JackLimited.Api/                  → Entry point (Program.cs)
│   ├── JackLimited.Application/          → Use cases, DTOs, Validators, MediatR (future)
│   ├── JackLimited.Domain/               → Entities, Value Objects
│   ├── JackLimited.Infrastructure/       → EF Core, Repositories, External Services
│   └── JackLimited.Tests/                → xUnit + FluentAssertions + NSubstitute
│       ├── UnitTests/
│       ├── IntegrationTests/            → WebApplicationFactory
│       └── Features/                     → BDD .feature files (SpecFlow recommended)
frontend/
└── docker-compose.yml


## COPILOT PROMPTS YOU MUST USE (Paste these exactly)

### 1. Create a new feature (TDD style)
/terminal
// @workspace: Create a new survey submission endpoint with full TDD
// First create failing integration test in IntegrationTests/SurveySubmissionTests.cs
// Then implement the minimal API endpoint and validator
// Use FluentValidation and return 201 Created with Location header

### 2. Generate BDD feature file first
// @workspace: Generate a SpecFlow .feature file for "Customer submits feedback survey"
// Include scenarios: Happy path, Invalid ratings (1-5), Missing required fields, NPS calculation

### 3. Generate unit test for a domain rule
// @workspace: Write a parameterized xUnit test for NPS calculation
// Promoters (9-10), Passives (7-8), Detractors (0-6)
// Test cases: 0 responses → 0, 10 promoters → 100, 5 detractors + 5 promoters → 0

### 4. Generate integration test with WebApplicationFactory
// @workspace: Create integration test using WebApplicationFactory
// POST /api/survey with valid payload → returns 201 and data is in PostgreSQL
// Use Testcontainers or actual dev database with unique schema

### 5. Refactor safely
// @workspace: Refactor this code to use Result pattern instead of exceptions
// All tests must still pass
// Use C# 12 primary constructors and records where possible

### 6. Generate Chart.js component with proper typing
// @workspace: Create Vue 3 + TypeScript bar chart for average ratings
// Use script setup, reactive props, and watch store.stats
// Must be fully typed and responsive

### 7. Add new validation rule
// @workspace: Add FluentValidation rule: Email must be valid if provided, Comments max 1000 chars
// Generate failing test first that sends invalid email and expects 400 with error message

## MANDATORY TESTING RULES (Copilot will remind you)

- Every endpoint → at least 1 integration test using `WebApplicationFactory`
- Every validator → unit tested with FluentValidation.TestHelper
- Every pure function (e.g., NPS calculation) → 100% coverage with Theory + MemberData
- All tests must run on CI (GitHub Actions already configured)
- Use `FluentAssertions` for readable assertions
- Use `NSubstitute` for mocking (not Moq)

Example assertion style:
```csharp
response.StatusCode.Should().Be(HttpStatusCode.Created);
response.Headers.Location!.ToString().Should().EndWith($"/api/survey/{createdId}");
savedResponse.LikelihoodToRecommend.Should().Be(9);

PERFORMANCE & SECURITY RULES (Copilot will enforce)

Always use AddRateLimiter() in production
Never return raw exceptions to client → use IProblemDetailsService
Use [ValidateAntiForgeryToken] if adding server-side rendering
Connection strings → never hardcode → use User Secrets or Azure Key Vault
CORS → only allow known origins (localhost:5173 and production domain)