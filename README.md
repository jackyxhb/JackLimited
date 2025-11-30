# JackLimited

A full-stack web application for collecting and analyzing customer feedback through Net Promoter Score (NPS) surveys.

## Features

- Submit customer feedback surveys with ratings and comments
- Real-time NPS calculation and analytics
- Rating distribution visualization
- Responsive Vue.js frontend
- ASP.NET Core Minimal API backend
- PostgreSQL database for production
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
- PostgreSQL (production) / In-Memory DB (development)
- FluentValidation for input validation
- Input sanitization for security

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

### Frontend Configuration

The frontend is configured to connect to `http://localhost:5173` for development. Update API endpoints in the stores if needed.

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

3. Open your browser to `http://localhost:5173`

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

### Frontend Unit Tests
```bash
cd frontend
npm run test:unit
```

### End-to-End Tests
```bash
cd frontend
npm run test:e2e
```

## API Endpoints

- `POST /api/survey` - Submit a survey response
- `GET /api/survey/nps` - Get current NPS score
- `GET /api/survey/average` - Get average rating
- `GET /api/survey/distribution` - Get rating distribution

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