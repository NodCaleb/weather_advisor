# Research: Unified Local Orchestration and Discovery

**Branch**: `002-aspire-orchestration-discovery` | **Date**: 2026-03-19  
**Purpose**: Resolve all NEEDS CLARIFICATION items from Technical Context before Phase 1 design.

---

## Decision 1 — Aspire Tooling Model for .NET 10

**Decision**: Use the NuGet SDK model (no `dotnet workload install aspire`). Install the Aspire CLI separately.

**Rationale**: Starting with Aspire 9, and fully for Aspire 13 on .NET 10, the workload model is dead. The AppHost project's `Sdk` attribute (`Aspire.AppHost.Sdk/13.1.3`) causes the .NET SDK to auto-restore Aspire tooling from NuGet at build time — the same as any package dependency. No global workload installation is required. The separate Aspire CLI (`aspire run`, `aspire init`) must be installed once per developer machine.

**Alternatives considered**:
- `dotnet workload install aspire` — this was the Aspire 8.x/early-9.x approach; no longer applicable for .NET 10 / Aspire 13.

**Install command** (one-time per machine):
```powershell
irm https://aspire.dev/install.ps1 | iex
aspire --version   # → 13.x.x+{sha}
```

---

## Decision 2 — AppHost Project Structure and NuGet Packages

**Decision**: Create `WeatherAdvisor.AppHost` project using `Sdk="Aspire.AppHost.Sdk/13.1.3"` targeting `net10.0`. Only one explicit NuGet package is required.

**Rationale**: The SDK encapsulates `Aspire.Hosting.AppHost` automatically — no explicit package reference needed for that. The only additional package required is `Aspire.Hosting.JavaScript` v13.1.3 (successor to the renamed and removed `Aspire.Hosting.NodeJs`) to support `AddViteApp`.

**Alternatives considered**:
- `Microsoft.NET.Sdk` with `Aspire.Hosting.AppHost` package — this was the pre-Aspire-13 pattern. The SDK approach is simpler and is now the canonical form.
- `Aspire.Hosting.NodeJs` — this package was renamed to `Aspire.Hosting.JavaScript` in Aspire 13 and no longer exists under the old name.

**AppHost `.csproj`**:
```xml
<Project Sdk="Aspire.AppHost.Sdk/13.1.3">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId><!-- generate new GUID --></UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\WeatherAdvisor.Api\WeatherAdvisor.Api.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.JavaScript" Version="13.1.3" />
  </ItemGroup>

</Project>
```

---

## Decision 3 — Frontend Resource API (`AddViteApp`)

**Decision**: Use `builder.AddViteApp("frontend", "../frontend")`. Do not use `AddNpmApp`.

**Rationale**: `AddNpmApp` is marked `[Obsolete]` in Aspire 13 and will be removed in Aspire 14. `AddViteApp` is the purpose-built replacement for Vite/React dev servers. It automatically:
- Registers an `http` endpoint and injects a `PORT` env var that Vite reads for its bind port
- Runs `npm run dev` (the `dev` script)
- Runs `npm install` before starting (no `WithNpmPackageInstallation()` needed)

**⚠️ Do NOT call `.WithHttpEndpoint()` on a `ViteApp` resource** — it already registers one, and adding another causes a duplicate endpoint error.

**Alternatives considered**:
- `AddNpmApp("frontend", "../frontend", "dev")` — marked obsolete; would trigger deprecation warnings and break in Aspire 14.

---

## Decision 4 — Service Discovery / Environment Variable Strategy

**Decision**: Use `.WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"))` combined with `.WithReference(api)` on the Vite resource.

**Rationale**:
- `WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"))` directly injects the dynamically assigned API URL as a `VITE_`-prefixed env var. Vite's dev server process reads all `VITE_*` OS environment variables at startup and makes them available via `import.meta.env.VITE_API_BASE_URL`. This satisfies FR-004 exactly: the TypeScript client (`weatherApiClient.ts`) already reads `import.meta.env.VITE_API_BASE_URL` and requires no changes.
- `WithReference(api)` additionally injects Aspire's standard discovery env vars (`WEATHERADVISOR_API_HTTP` and the legacy `services__weatheradvisor-api__http__0`) for forward compatibility and to formally express the dependency in the resource graph (enables the Aspire dashboard to display the dependency link).

**Aspire 13 env var convention** (injected by `WithReference`):
- Simplified form (new in Aspire 13): `WEATHERADVISOR_API_HTTP=http://localhost:{port}`
- Legacy form (backwards compat): `services__weatheradvisor-api__http__0=http://localhost:{port}`
- `WEATHERADVISOR_API_HTTPS=https://localhost:{port}` (if HTTPS endpoint also registered)

**Why not Vite proxy only?**: The spec (FR-004) explicitly requires `VITE_API_BASE_URL` to be used by the TypeScript client. A Vite proxy approach would require changing `weatherApiClient.ts` to use relative paths and would not use `VITE_API_BASE_URL` — this contradicts the spec requirement.

**Alternatives considered**:
- Vite proxy only — would eliminate CORS but contradicts FR-004 which mandates `VITE_API_BASE_URL` flow.
- `WithReference(api)` alone without `WithEnvironment` — `WEATHERADVISOR_API_HTTP` is injected, but this is not a `VITE_*` var and Vite would not expose it via `import.meta.env`.

---

## Decision 5 — User Secrets Forwarding (WeatherAdvisor.Api)

**Decision**: No AppHost configuration needed. User secrets load automatically.

**Rationale**: When `AddProject<Projects.WeatherAdvisor_Api>` spawns the API process, Aspire sets `ASPNETCORE_ENVIRONMENT=Development`. ASP.NET Core then automatically loads user secrets for any project that has a `<UserSecretsId>` in its `.csproj`. The API process reads its own secrets natively — Aspire does not need to forward them. The AppHost only needs its own `<UserSecretsId>` if the AppHost itself requires secrets (e.g., secrets for provisioned container resources — not applicable here).

**Prerequisites for developers**: Before first launch, run:
```powershell
cd backend/WeatherAdvisor.Api
dotnet user-secrets init        # if UserSecretsId not yet in .csproj
dotnet user-secrets set "OpenMeteo:ApiKey" "<value-if-needed>"
```

**Alternatives considered**:
- Forwarding secrets via AppHost `AddParameter` — adds unnecessary complexity; the API process already loads its own secrets directly.

---

## Decision 6 — ServiceDefaults Project

**Decision**: Deferred. Do not create `WeatherAdvisor.ServiceDefaults` for this feature.

**Rationale**: ServiceDefaults provides OpenTelemetry traces/metrics, default health check endpoints, and HTTP resilience policies. None of these are required for the P1 or P2 scenarios (single-command startup, service discovery). The Aspire dashboard will still show both components, their endpoints, and console logs without ServiceDefaults. Adding ServiceDefaults would mean creating a third project and modifying `Program.cs` — unjustified complexity for what is functionally an infrastructure-only developer-experience feature.

**When to add**: A future feature that requires dashboard health status (`/health` → Healthy/Unhealthy) or structured OpenTelemetry traces should add ServiceDefaults then, scoped to that feature.

**Alternatives considered**:
- Add ServiceDefaults now — adds a third project, modifies `Program.cs`, and provides no measurable benefit to the acceptance criteria in this spec.

---

## Decision 7 — CORS Strategy Under Dynamic Port Assignment

**Decision**: Update the API's CORS policy to use `AllowAnyOrigin()` when running in Development (replacing the hardcoded `http://localhost:5173` origin).

**Rationale**: The Aspire AppHost assigns ports dynamically. The frontend Vite dev server will receive a different port on each run. The current CORS policy (`WithOrigins("http://localhost:5173")`) will reject every cross-origin request from the Vite-managed frontend. Updating to `AllowAnyOrigin()` in Development is safe because:
1. It only applies in the Development environment (ASPNETCORE_ENVIRONMENT=Development).
2. No production deployment is in scope (Constitution §IV and spec Out of Scope).
3. The API handles no sensitive user data (stateless demo application).

**Alternatives considered**:
- Inject CORS_FRONTEND_ORIGIN from AppHost into the API — creates a circular dependency (frontend endpoint not allocated when AppHost evaluates API environment vars) and adds unnecessary complexity. Rejected.
- Vite proxy (browsers make same-origin requests, no CORS needed) — contradicts FR-004 which requires `VITE_API_BASE_URL` used by the TypeScript client. Rejected.
- Keep hardcoded `http://localhost:5173` — will break when Aspire assigns a different frontend port. Rejected.

---

## Decision 8 — New Project Placement and Solution File

**Decision**: Place `WeatherAdvisor.AppHost` in `backend/WeatherAdvisor.AppHost/` and add it to `backend/WeatherAdvisor.slnx`.

**Rationale**: All .NET projects live under `backend/`. Placing AppHost alongside the API and Test projects maintains the existing repository structure convention. The `WeatherAdvisor.slnx` solution file should reference AppHost so it is visible in IDEs and builds correctly.

**Project layout after this feature**:
```text
backend/
├── WeatherAdvisor.slnx
├── WeatherAdvisor.Api/
├── WeatherAdvisor.AppHost/      ← new
└── WeatherAdvisor.Tests/
```

**Alternatives considered**:
- Placing AppHost at repository root — non-standard for this project; all .NET code lives under `backend/`.
- Separate solution file — unnecessary; adding to the existing `.slnx` is simpler.

---

## Decision 9 — `UserSecretsId` in `WeatherAdvisor.Api.csproj`

**Decision**: Add `<UserSecretsId>` to `WeatherAdvisor.Api.csproj` to explicitly anchor user secrets to the project.

**Rationale**: The API uses .NET User Secrets for local configuration (API keys, etc.). For Aspire to correctly identify and load the right secrets store, the project must have an explicit `<UserSecretsId>`. Without it, `dotnet user-secrets` uses a hash of the project path, which can drift if the project is moved. A stable GUID ensures the secrets store is portable.

**If already set**: Check with `grep UserSecretsId backend/WeatherAdvisor.Api/WeatherAdvisor.Api.csproj`. If not present, `dotnet user-secrets init` will add one.
