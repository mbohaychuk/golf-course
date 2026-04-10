# Miquelon Hills Golf Course

A full-stack website and tee time booking platform for **Miquelon Hills Golf Course**, located near Miquelon Lake Provincial Park — about an hour east of Edmonton, Alberta.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | [Nuxt 3](https://nuxt.com/) (Vue 3) with SSR |
| Styling | [Tailwind CSS](https://tailwindcss.com/) |
| API | [ASP.NET Core](https://dotnet.microsoft.com/) (.NET 10) |
| Database | SQL Server via Entity Framework Core |
| Auth | ASP.NET Identity + JWT |
| Frontend Tests | [Vitest](https://vitest.dev/) + Vue Test Utils |
| API Tests | [xUnit](https://xunit.net/) + WebApplicationFactory (SQLite in-memory) |

## Project Structure

```
MiquelonGolf/
├── MiquelonGolf.Web/          # Nuxt 3 frontend
│   ├── pages/                 # Route pages (home, book, contact, golf, events, rv, admin)
│   ├── components/            # Vue components (booking wizard, UI primitives, nav, footer)
│   ├── composables/           # Shared logic (useApi, useAuth, useToast)
│   ├── layouts/               # App layouts (default, admin)
│   └── tests/                 # Vitest component & page tests
├── MiquelonGolf.Api/          # ASP.NET Core Web API
│   ├── Controllers/           # REST endpoints (bookings, tee times, events, auth, admin)
│   ├── Models/                # EF Core entities (Booking, TeeTimeSlot, Member, etc.)
│   ├── Services/              # Business logic (tee time management, slot generation, JWT)
│   ├── DTOs/                  # Request/response data transfer objects
│   └── Migrations/            # EF Core database migrations
├── MiquelonGolf.Api.Tests/    # xUnit integration tests for the API
└── MiquelonGolf.slnx          # Solution file
```

## Features

### Public Site
- **Home** — hero, announcements, course highlights
- **Book a Tee Time** — 3-step booking wizard with round type selector, calendar, and time slot picker
- **The Course** — course layout, hole details, and course info
- **Fees** — green fees and rate information
- **Hours** — operating hours with holiday/seasonal overrides
- **Events** — upcoming events and tournaments
- **RV Sites** — seasonal RV site information
- **Contact** — contact form and location info

### Admin Panel
- **Tee Sheet** — daily tee sheet with dual-column layout, flow-through support, inline edit/move/no-show
- **Bookings** — search, inline edit, and no-show tracking
- **Slot Generation** — configurable automatic tee time slot generation with booking windows
- **Settings** — booking and site configuration
- **Content Management** — editable site content
- **Hours & Events** — manage operating hours, holidays, and events
- **Members** — member management

## Getting Started

### Prerequisites

- [Node.js](https://nodejs.org/) (v20+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full instance)

### Frontend

```bash
cd MiquelonGolf.Web
npm install
npm run dev
```

The dev server starts at `http://localhost:3000` by default.

### API

```bash
cd MiquelonGolf.Api
dotnet run
```

The API starts at `http://localhost:5151` by default. Swagger UI is available at `/swagger`.

### Database

Apply EF Core migrations to set up the database:

```bash
cd MiquelonGolf.Api
dotnet ef database update
```

### Running Tests

```bash
# Frontend tests
cd MiquelonGolf.Web
npm test

# API tests
cd MiquelonGolf.Api.Tests
dotnet test
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `API_BASE` | Base URL for the backend API | `http://localhost:5151` |

API configuration (connection string, JWT settings) is managed in `appsettings.json` / `appsettings.Development.json`.
