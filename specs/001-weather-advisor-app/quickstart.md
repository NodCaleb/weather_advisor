# Quickstart: Weather Advisor

**Branch**: `001-weather-advisor-app` | **Date**: March 12, 2026

This guide covers setting up and running the Weather Advisor application locally: the .NET 10 backend and the React (TypeScript + Vite) frontend.

---

## Prerequisites

| Tool | Minimum Version | Check |
|------|----------------|-------|
| .NET SDK | 10.0 | `dotnet --version` |
| Node.js | 20 LTS | `node --version` |
| npm | 10+ | `npm --version` |

No API keys, database, or paid tooling are required. Open-Meteo is a free public API.

---

## 1. Clone and Navigate

```bash
git clone <repo-url>
cd weather_advisor
```

---

## 2. Backend Setup

### 2.1 Configuration

The backend has no secrets for this feature (Open-Meteo requires no API key). The only configuration is the Open-Meteo base URLs and the HTTP timeout, which have sensible defaults defined in `appsettings.json`.

Copy the development settings example if it does not already exist:

```bash
cp backend/WeatherAdvisor.Api/appsettings.Development.example.json \
   backend/WeatherAdvisor.Api/appsettings.Development.json
```

> `appsettings.Development.json` is git-ignored. The example file is safe to commit and contains no secrets.

**Default settings** (in `appsettings.json`):
```json
{
  "OpenMeteo": {
    "GeocodingBaseUrl": "https://geocoding-api.open-meteo.com",
    "ForecastBaseUrl": "https://api.open-meteo.com",
    "TimeoutSeconds": 5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WeatherAdvisor": "Debug"
    }
  }
}
```

To override log verbosity locally, set the desired level in `appsettings.Development.json`.

### 2.2 Restore and Run

```bash
cd backend/WeatherAdvisor.Api
dotnet restore
dotnet run
```

The API will start at **`http://localhost:5000`** by default.

To verify:
```bash
curl "http://localhost:5000/weather?city=London"
```

### 2.3 Run Backend Tests

```bash
cd backend
dotnet test
```

---

## 3. Frontend Setup

### 3.1 Configuration

```bash
cd frontend
cp .env.example .env.local
```

`.env.example` content (safe to commit, no secrets):
```
VITE_API_BASE_URL=http://localhost:5000
```

Edit `.env.local` if your backend runs on a different port.

### 3.2 Install and Run

```bash
npm install
npm run dev
```

The frontend will start at **`http://localhost:5173`** (Vite default).

Open your browser at `http://localhost:5173` to use the application.

### 3.3 Run Frontend Tests

```bash
npm run test
```

---

## 4. Running Both Together

Open two terminal windows:

**Terminal 1 — Backend**:
```bash
cd backend/WeatherAdvisor.Api
dotnet run
```

**Terminal 2 — Frontend**:
```bash
cd frontend
npm run dev
```

Then open `http://localhost:5173` in your browser.

---

## 5. Verify End-to-End

1. Enter a city name (e.g., `Paris`) and click **Search**.
2. Verify weather data is displayed: temperature, wind speed, precipitation probability, and condition label.
3. Select an activity (e.g., `Cycling`).
4. Verify a recommendation and explanation appear.
5. Switch to a different activity — confirm the recommendation updates immediately without a page reload.
6. Enter a non-existent city (e.g., `Xyzabc123`) — confirm a "City not found" error message appears.

---

## 6. Environment Variables Reference

### Backend

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` for dev logging and CORS dev policy |
| `ASPNETCORE_URLS` | `http://localhost:5000` | Binding address |

> No API keys or secrets are required for this feature. All Open-Meteo endpoints are public and unauthenticated.

### Frontend

| Variable | Default | Required | Description |
|----------|---------|----------|-------------|
| `VITE_API_BASE_URL` | *(none)* | Yes | Base URL of the .NET backend API |

---

## 7. Project Structure Overview

```text
weather_advisor/
├── backend/
│   ├── WeatherAdvisor.Api/          # ASP.NET Core Web API
│   └── WeatherAdvisor.Tests/        # xUnit test project
├── frontend/
│   ├── src/                         # React TypeScript source
│   └── tests/                       # Vitest component tests
└── specs/
    └── 001-weather-advisor-app/     # Feature spec and design docs
```
