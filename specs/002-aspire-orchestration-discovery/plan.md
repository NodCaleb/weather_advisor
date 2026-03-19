# Implementation Plan: Unified Local Orchestration and Discovery

**Branch**: `002-aspire-orchestration-discovery` | **Date**: 2026-03-19 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/002-aspire-orchestration-discovery/spec.md`

## Summary

Add a .NET Aspire AppHost project (`WeatherAdvisor.AppHost`) that starts both the WeatherAdvisor.Api backend and the Vite/React frontend with a single command. The AppHost injects the backend's dynamically assigned URL as `VITE_API_BASE_URL` into the Vite dev server process, enabling service discovery without hardcoded ports. The existing CORS policy in the API is updated to `AllowAnyOrigin()` in Development to accommodate the frontend's dynamic port assignment. No production deployment, CI/CD integration, ServiceDefaults project, or Docker requirement is introduced.

## Technical Context

**Language/Version**: .NET 10.0 (C#) + TypeScript / Node.js 20 LTS  
**Primary Dependencies**: .NET Aspire 13.1.3 (`Aspire.AppHost.Sdk/13.1.3`, `Aspire.Hosting.JavaScript` v13.1.3), Vite 6.x (existing), Aspire CLI 13.x  
**Storage**: N/A — no persistence introduced (Constitution §IV)  
**Testing**: xUnit / `dotnet test` (backend, existing), Vitest / `npm run test` (frontend, existing) — no new test projects for this feature  
**Target Platform**: Local developer workstation (Windows / macOS / Linux)  
**Project Type**: Developer tooling / local orchestration host  
**Performance Goals**: Full stack startup in under 30 seconds from clean state  
**Constraints**: No fixed ports; secrets via .NET User Secrets only; no Docker required; no secrets in source control  
**Scale/Scope**: Two components — WeatherAdvisor.Api (.NET) + Vite/React frontend (npm)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design — all gates PASS.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Specification-First Delivery | ✅ PASS | Full spec with acceptance criteria exists; plan and tasks follow before any code |
| II | Clear Separation of Concerns | ✅ PASS | No change to backend/frontend layer boundaries; AppHost is purely infrastructure |
| III | API-First Contract Discipline | ✅ PASS | No new API endpoints. Existing `/weather` and `/recommendation` contracts from feature 001 unchanged |
| IV | No-Persistence Constraint | ✅ PASS | No database, cache, or file storage introduced |
| V | Security and Secrets Management | ✅ PASS | Secrets via .NET User Secrets; `VITE_API_BASE_URL` is a URL not a secret; no secrets in source control |
| VI | Simplicity and Maintainability | ✅ PASS | One new project file + one `Program.cs` (~10 lines). ServiceDefaults explicitly deferred. CORS updated minimally |
| VII | Testability and Verification | ✅ PASS | Existing unit/integration tests unaffected; AppHost configuration is not unit-tested (standard Aspire practice for simple topologies) |
| VIII | Reliability and Failure Handling | ✅ PASS | No change to API error handling; Aspire dashboard provides failure visibility (SC-004) |
| IX | Observability for Development | ✅ PASS | Aspire dashboard provides component health, logs, and endpoint visibility; ServiceDefaults telemetry deferred as justified in research |
| X | Developer Experience | ✅ PASS | This feature IS the developer experience improvement — single-command startup with documented quickstart |
| XI | Frontend Principles | ✅ PASS | Frontend continues consuming backend exclusively; `VITE_API_BASE_URL` env var strategy requires no CORS workaround in TypeScript code |
| XII | Backend Principles | ✅ PASS | No layer changes; CORS update is minimal and scoped to Development only |

## Project Structure

### Documentation (this feature)

```text
specs/002-aspire-orchestration-discovery/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/
│   └── orchestration.md # Phase 1 output — env var and CORS contracts
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code Changes (repository root)

```text
backend/
├── WeatherAdvisor.slnx               ← modified: add AppHost project reference
├── WeatherAdvisor.Api/
│   ├── WeatherAdvisor.Api.csproj     ← modified: add <UserSecretsId> if missing
│   └── Program.cs                   ← modified: update CORS to AllowAnyOrigin in Development
├── WeatherAdvisor.AppHost/           ← new project
│   ├── WeatherAdvisor.AppHost.csproj
│   └── Program.cs
└── WeatherAdvisor.Tests/             (unchanged)

frontend/
└── vite.config.ts                    (unchanged — VITE_API_BASE_URL read natively by Vite dev server)
```

**Structure Decision**: Web application pattern (existing `backend/` + `frontend/` layout). New AppHost project placed under `backend/` to maintain .NET project co-location convention. No frontend file changes required for this feature — `weatherApiClient.ts` already reads `import.meta.env.VITE_API_BASE_URL`.

## Complexity Tracking

> No constitution violations. This section is present for completeness only.

No unjustified complexity was introduced. All decisions in research.md reflect the minimum required to satisfy the spec requirements:

- ServiceDefaults project: explicitly deferred (see research Decision 6)
- Vite proxy approach: rejected because it would require changing `weatherApiClient.ts` and contradicts FR-004
- Fixed port declaration: rejected by spec design decision (dynamic ports required)

---

## Implementation Notes

### AppHost Program.cs (target state)

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.WeatherAdvisor_Api>("weatheradvisor-api");

builder.AddViteApp("frontend", "../frontend")
    .WithReference(api)
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"));

builder.Build().Run();
```

### CORS update in WeatherAdvisor.Api/Program.cs (target state)

Replace the current hardcoded origin `"http://localhost:5173"` with `AllowAnyOrigin()` in Development:

```csharp
const string CorsPolicyName = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});
```

### Required NuGet packages

| Project | Package | Version |
|---------|---------|---------|
| `WeatherAdvisor.AppHost` | `Aspire.Hosting.JavaScript` | 13.1.3 |

`Aspire.Hosting.AppHost` is NOT referenced explicitly — it is pulled in automatically by the `Aspire.AppHost.Sdk/13.1.3` SDK.

### Solution file update

Add the AppHost project to `backend/WeatherAdvisor.slnx`:

```xml
<Solution>
  <Project Path="WeatherAdvisor.Api/WeatherAdvisor.Api.csproj" />
  <Project Path="WeatherAdvisor.AppHost/WeatherAdvisor.AppHost.csproj" />
  <Project Path="WeatherAdvisor.Tests/WeatherAdvisor.Tests.csproj" />
</Solution>
```
