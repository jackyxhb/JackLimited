# Release Notes

## Version 1.0.4 - Maintenance Release (December 2, 2025)

### Updates
- **Framework Upgrade**: Migrated backend from .NET 9.0 to .NET 8.0 for stability and LTS support.
- **Package Updates**: Updated NuGet packages to latest compatible versions, including security patches for Npgsql (CVE-2024-32655).
- **Docker Improvements**: Switched to Ubuntu-based .NET 8.0 images to address zlib vulnerabilities (CVE-2023-45853).
- **CI/CD Enhancements**: Added Trivy vulnerability scanning in CI pipeline; implemented automatic version bumping and Git tagging.
- **Security**: Enhanced container security with updated base images and dependency patches.

### Technical Implementation
- **Frontend**: Vue 3 with TypeScript, Vite build tool, Pinia state management, Vue Router, Playwright E2E testing, Vitest unit testing.
- **Backend**: ASP.NET Core Minimal API (.NET 8.0), Entity Framework Core 8.0.4, FluentValidation, input sanitization.
- **Architecture**: Clean Architecture with separate Domain, Application, Infrastructure, and API layers.
- **Security**: Input sanitization to prevent XSS, proper error handling, vulnerability scanning.

### Known Issues
- None reported.

### Future Enhancements
- User authentication and authorization
- Survey templates and customization
- Advanced analytics and reporting
- Email notifications
- Multi-language support

## Version 1.0.0 - Initial Release (November 30, 2025)

### New Features
- **Survey Submission**: Users can submit feedback surveys with likelihood to recommend ratings (0-10) and optional comments and email.
- **NPS Calculation**: Real-time Net Promoter Score calculation based on submitted ratings.
- **Analytics Dashboard**: View NPS score, average rating, and rating distribution charts.
- **Responsive Design**: Mobile-friendly Vue.js frontend with theme toggle functionality.
- **Data Visualization**: Interactive charts for NPS breakdown (Promoters, Passives, Detractors) and rating distribution.
- **Input Validation**: Server-side validation and sanitization for survey submissions.
- **CORS Support**: Configured for cross-origin requests between frontend and backend.
- **Database Integration**: PostgreSQL for production data storage, in-memory database for development/testing.

### Technical Implementation
- **Frontend**: Vue 3 with TypeScript, Vite build tool, Pinia state management, Vue Router, Playwright E2E testing, Vitest unit testing.
- **Backend**: ASP.NET Core Minimal API, Entity Framework Core, FluentValidation, input sanitization.
- **Architecture**: Clean Architecture with separate Domain, Application, Infrastructure, and API layers.
- **Security**: Input sanitization to prevent XSS, proper error handling.

### API Endpoints
- `POST /api/survey` - Submit survey response
- `GET /api/survey/nps` - Retrieve current NPS score
- `GET /api/survey/average` - Get average rating
- `GET /api/survey/distribution` - Get rating distribution data

### Known Issues
- None reported at initial release.

### Future Enhancements
- User authentication and authorization
- Survey templates and customization
- Advanced analytics and reporting
- Email notifications
- Multi-language support

### Installation and Setup
See README.md for detailed installation and running instructions.

### Testing
- Unit tests for backend logic
- E2E tests for frontend functionality using Playwright
- Cross-browser testing (Chromium, Firefox, WebKit)

This release marks the initial deployment of the JackLimited customer feedback platform, providing a complete solution for collecting and analyzing NPS data.