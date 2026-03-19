# weather_advisor Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-12

## Active Technologies
- .NET 10.0 (C#) + TypeScript / Node.js 20 LTS + .NET Aspire 13.1.3 (`Aspire.AppHost.Sdk/13.1.3`, `Aspire.Hosting.JavaScript` v13.1.3), Vite 6.x (existing), Aspire CLI 13.x (002-aspire-orchestration-discovery)
- N/A — no persistence introduced (Constitution §IV) (002-aspire-orchestration-discovery)

- TypeScript 5 / React 18 (frontend) + C# 14 / .NET 10 LTS (backend) + React 18, Vite, Axios (frontend); ASP.NET Core Web API, xUnit, Moq (backend); Open-Meteo Geocoding API + Forecast API (external, no key required) (001-weather-advisor-app)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

npm test; npm run lint

## Code Style

TypeScript 5 / React 18 (frontend) + C# 14 / .NET 10 LTS (backend): Follow standard conventions

## Recent Changes
- 002-aspire-orchestration-discovery: Added .NET 10.0 (C#) + TypeScript / Node.js 20 LTS + .NET Aspire 13.1.3 (`Aspire.AppHost.Sdk/13.1.3`, `Aspire.Hosting.JavaScript` v13.1.3), Vite 6.x (existing), Aspire CLI 13.x

- 001-weather-advisor-app: Added TypeScript 5 / React 18 (frontend) + C# 14 / .NET 10 LTS (backend) + React 18, Vite, Axios (frontend); ASP.NET Core Web API, xUnit, Moq (backend); Open-Meteo Geocoding API + Forecast API (external, no key required)

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
