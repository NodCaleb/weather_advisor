# Orchestration Contracts: Unified Local Orchestration and Discovery

**Branch**: `002-aspire-orchestration-discovery` | **Date**: 2026-03-19  
**Spec**: [spec.md](../spec.md)

> This contract defines the environment variable interface injected by the Aspire AppHost into each managed component. All values are dynamically allocated at runtime; no hardcoded ports appear in any source file. Implementation MUST match these contracts exactly; any change requires a spec update first.

---

## Environment Variable Contract

### WeatherAdvisor.Api (ProjectResource)

The API project (`WeatherAdvisor.Api`) is started as a .NET process by the AppHost. No additional environment variables are injected by the AppHost; the project loads its own configuration through standard ASP.NET Core mechanisms.

| Variable | Source | Value | Notes |
|----------|--------|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | Aspire (automatic) | `Development` | Triggers user-secrets loading |
| `ASPNETCORE_URLS` | Aspire (dynamic port) | `http://localhost:{port}` | Port dynamically assigned by Aspire; overrides any launchSettings value |

> User secrets (`dotnet user-secrets`) are loaded by the API process itself because `ASPNETCORE_ENVIRONMENT=Development` and the project has a `<UserSecretsId>`. **No forwarding configuration is required in the AppHost.**

---

### frontend (ViteAppResource)

The Vite/React dev server is started as an npm process by the AppHost via `AddViteApp`. The following environment variables are injected into the npm process before startup.

| Variable | Source | Value | Notes |
|----------|--------|-------|-------|
| `PORT` | Aspire (automatic, via `AddViteApp`) | `{dynamicPort}` | Vite reads `PORT` to determine its bind port |
| `VITE_API_BASE_URL` | AppHost `WithEnvironment` | `http://localhost:{api-port}` | **Service discovery variable.** Read by `weatherApiClient.ts` via `import.meta.env.VITE_API_BASE_URL`. |
| `WEATHERADVISOR_API_HTTP` | Aspire (via `WithReference`) | `http://localhost:{api-port}` | Aspire 13 simplified polyglot service discovery var (same value as `VITE_API_BASE_URL`) |
| `services__weatheradvisor-api__http__0` | Aspire (via `WithReference`) | `http://localhost:{api-port}` | Legacy .NET service discovery convention; included for forwards compatibility |

---

## AppHost Resource Graph

```
DistributedApplication
├── weatheradvisor-api  [ProjectResource: WeatherAdvisor.Api]
│    └── user-secrets:  loaded natively by the API process
└── frontend            [ViteAppResource: ../frontend]
     ├── depends-on:    weatheradvisor-api
     ├── env:           VITE_API_BASE_URL  → weatheradvisor-api:http endpoint
     └── env:           WEATHERADVISOR_API_HTTP → weatheradvisor-api:http endpoint
```

---

## Frontend TypeScript Client Contract

The `weatherApiClient.ts` service (existing, no change required) reads the API base URL as follows:

```typescript
const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
```

| Scenario | `VITE_API_BASE_URL` value | Effective base URL |
|----------|--------------------------|-------------------|
| Aspire managed (this feature) | `http://localhost:{dynamic}` | Dynamic Aspire-assigned port |
| Manual dev (standalone `npm run dev`) | unset | `http://localhost:5000` (fallback) |
| Unit tests (Vitest) | unset | `http://localhost:5000` (fallback, not called) |

**Contract invariant**: The fallback `http://localhost:5000` matches the existing standalone backend startup convention (see `001-weather-advisor-app` quickstart). This ensures backward compatibility with manual dev workflows (FR-007).

---

## CORS Contract

Under Aspire orchestration, the frontend's Vite dev server is assigned a dynamic port unknown to the API at startup time. The API's CORS policy must be updated to permit the cross-origin request from any localhost origin in the Development environment.

| Environment | Policy | Allowed Origins |
|-------------|--------|-----------------|
| `Development` | `FrontendDev` | `AllowAnyOrigin()` |
| Non-Development | `FrontendDev` (unchanged) | `http://localhost:5173` (unchanged hardcoded fallback) |

> **Security note**: `AllowAnyOrigin()` is restricted to the Development environment. No production deployment is in scope for this feature. The application handles no user authentication, sessions, or sensitive personal data.

---

## Port Assignment Summary

| Component | Port Assignment | How Determined |
|-----------|----------------|----------------|
| WeatherAdvisor.Api | Dynamic (Aspire) | Aspire assigns at startup; value printed to Aspire dashboard and terminal |
| frontend (Vite dev server) | Dynamic (Aspire via `PORT`) | Aspire assigns; Vite reads `PORT` env var to set its bind address |
| Aspire Dashboard | Dynamic | URL (with token) printed to terminal at startup |

> No fixed ports are declared in the AppHost — this is the explicit design per the feature spec clarification: "Dynamic port assignment by Aspire (no fixed ports declared in AppHost)."
