# Implementation Plan: Weather Advisor

**Branch**: `001-weather-advisor-app` | **Date**: March 12, 2026 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/001-weather-advisor-app/spec.md`

## Summary

A React (TypeScript) SPA paired with a .NET 10 Web API backend that retrieves current weather from Open-Meteo for a user-specified city and evaluates it against per-activity suitability rules (Running, Cycling, Picnic, Walking) to produce a Suitable / Caution / Not Recommended verdict with an explanatory message. The backend proxies all Open-Meteo requests (geocoding + forecast); the frontend is a pure presentation layer. No persistence, no authentication, metric units only.

## Technical Context

**Language/Version**: TypeScript 5 / React 18 (frontend) + C# 14 / .NET 10 LTS (backend)  
**Primary Dependencies**: React 18, Vite, Axios (frontend); ASP.NET Core Web API, xUnit, Moq (backend); Open-Meteo Geocoding API + Forecast API (external, no key required)  
**Storage**: N/A — no persistence required or permitted (Constitution IV)  
**Testing**: Vitest + React Testing Library (frontend); xUnit + Moq (backend)  
**Target Platform**: Modern web browser (React SPA) + Linux/Windows web server (.NET 10)  
**Project Type**: Web application — React SPA frontend + ASP.NET Core REST API backend  
**Performance Goals**: Weather retrieval ≤ 3 s under normal conditions (SC-001); recommendation update ≤ 1 s after weather load (SC-002)  
**Constraints**: 5-second hard timeout on all Open-Meteo API calls; metric units only (°C, km/h, mm); no auth; no storage  
**Scale/Scope**: Demo/prototype; single-user, single-city, single-activity per interaction; no multi-tenancy

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Gate Condition | Status | Notes |
|---|-----------|----------------|--------|-------|
| I | Specification-First | Approved spec exists before implementation | ✅ PASS | spec.md complete with FR, SC, and acceptance scenarios |
| II | Separation of Concerns | Frontend calls backend exclusively; no direct external API calls from browser | ✅ PASS | Backend proxies Open-Meteo; frontend only calls the internal REST API |
| III | API-First Contract Discipline | HTTP contracts documented before implementation | ✅ PASS | Contracts defined in `/contracts/api.md` (Phase 1 output) |
| IV | No-Persistence Constraint | No database, cache store, or file storage introduced | ✅ PASS | FR-015 explicitly prohibits all storage |
| V | Security & Secrets | Secrets managed via environment variables; none committed | ✅ PASS | Open-Meteo requires no API key; no secrets required for this feature |
| VI | Simplicity | Simplest solution that meets spec; no unjustified abstractions | ✅ PASS | Two-project structure; no extra services or infrastructure |
| VII | Testability | External API behind injectable interface; recommendation logic unit-testable in isolation | ✅ PASS | `IOpenMeteoClient` abstraction enables testing without HTTP |
| VIII | Reliability | All failure modes (timeout, city-not-found, missing fields, API unavailable) specified | ✅ PASS | FR-008 – FR-012 cover all failure scenarios |
| IX | Observability | Structured logging in .NET for integration events; level configurable | ✅ PASS | Standard `ILogger<T>` with `appsettings.Development.json` level control |
| X | Developer Experience | Quickstart.md documents local setup; `.env.example` / settings example provided | ✅ PASS | `quickstart.md` generated in Phase 1 |
| XI | Frontend Principles | Loading, error, and empty states handled in every API-consuming component | ✅ PASS | User Stories 1–5 acceptance scenarios cover all three states |
| XII | Backend Principles | Controllers handle only HTTP; service layer owns logic; `IHttpClientFactory` used | ✅ PASS | Layered architecture with `OpenMeteoClient` behind interface |

**Post-Phase 1 re-check**: All gates remain PASS. No violations. No complexity tracking required.

## Project Structure

### Documentation (this feature)

```text
specs/001-weather-advisor-app/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── api.md           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── WeatherAdvisor.Api/
│   ├── Controllers/
│   │   └── WeatherController.cs
│   ├── Models/
│   │   ├── Requests/
│   │   │   └── GetWeatherRequest.cs
│   │   ├── Responses/
│   │   │   ├── WeatherResponse.cs
│   │   │   └── RecommendationResponse.cs
│   │   └── ErrorResponse.cs
│   ├── Services/
│   │   ├── IWeatherService.cs
│   │   ├── WeatherService.cs
│   │   ├── IActivityAdvisorService.cs
│   │   └── ActivityAdvisorService.cs
│   ├── Integration/
│   │   ├── IOpenMeteoClient.cs
│   │   ├── OpenMeteoClient.cs
│   │   └── Models/
│   │       ├── GeocodingResponse.cs
│   │       └── ForecastResponse.cs
│   ├── Configuration/
│   │   └── OpenMeteoOptions.cs
│   └── Program.cs
└── WeatherAdvisor.Tests/
    ├── Services/
    │   ├── WeatherServiceTests.cs
    │   └── ActivityAdvisorServiceTests.cs
    └── Integration/
        └── OpenMeteoClientTests.cs

frontend/
├── src/
│   ├── components/
│   │   ├── CitySearch.tsx
│   │   ├── WeatherCard.tsx
│   │   ├── ActivitySelector.tsx
│   │   └── RecommendationCard.tsx
│   ├── pages/
│   │   └── HomePage.tsx
│   ├── services/
│   │   └── weatherApiClient.ts
│   ├── hooks/
│   │   └── useWeather.ts
│   └── types/
│       └── models.ts
└── tests/
    └── components/
```

**Structure Decision**: Web application layout — `backend/` hosts the ASP.NET Core solution; `frontend/` hosts the Vite + React SPA. Clean tier separation enforced per Constitution Principles II and XII. Each project is independently runnable and testable.
