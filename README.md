# Weather Advisor

A sample web application that helps users decide whether planned outdoor activities are suitable based on current weather conditions. Built as a demonstration project to showcase the possibilities of **[Spec-Kit](https://github.com/eurochriskelly/pp-spec-kit)** — a spec-driven development workflow for GitHub Copilot.

## What is this?

Weather Advisor lets users:
- Look up current weather for any city (temperature, wind speed, precipitation, condition label)
- Select an outdoor activity (Running, Cycling, Picnic, Walking)
- Receive a suitability recommendation (Suitable / Caution / Not Recommended) with a plain-language explanation

The entire application was designed and built using Spec-Kit's structured workflow: clarification → specification → data model → implementation plan → tasks → code. For a step-by-step account of how it was done, see [HOW_I_BUILT_THIS.md](HOW_I_BUILT_THIS.md).

**Stack**: React (TypeScript + Vite) frontend · .NET 10 Web API backend · [Open-Meteo](https://open-meteo.com/) (free, no API key required)

---

## Quick Start

### Prerequisites

| Tool | Minimum Version | Check |
|------|----------------|-------|
| .NET SDK | 10.0 | `dotnet --version` |
| Node.js | 20 LTS | `node --version` |
| npm | 10+ | `npm --version` |

### 1. Clone

```bash
git clone <repo-url>
cd weather_advisor
```

### 2. Backend

```bash
cp backend/WeatherAdvisor.Api/appsettings.Development.example.json \
   backend/WeatherAdvisor.Api/appsettings.Development.json

cd backend/WeatherAdvisor.Api
dotnet restore
dotnet run
```

API starts at **`http://localhost:5000`**.

### 3. Frontend

```bash
cd frontend
cp .env.example .env.local   # sets VITE_API_BASE_URL=http://localhost:5000
npm install
npm run dev
```

App starts at **`http://localhost:5173`** — open it in your browser.

### 4. Run Tests

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npm run test
```

> For a full walkthrough including environment variables and end-to-end verification steps, see [specs/001-weather-advisor-app/quickstart.md](specs/001-weather-advisor-app/quickstart.md).

---

## Project Structure

```text
weather_advisor/
├── backend/
│   ├── WeatherAdvisor.Api/          # ASP.NET Core Web API
│   └── WeatherAdvisor.Tests/        # xUnit test project
├── frontend/
│   ├── src/                         # React TypeScript source
│   └── tests/                       # Vitest component tests
└── specs/
    └── 001-weather-advisor-app/     # Spec-Kit design artifacts
```
