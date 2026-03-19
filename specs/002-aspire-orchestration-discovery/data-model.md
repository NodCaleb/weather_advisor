# Data Model: Unified Local Orchestration and Discovery

**Branch**: `002-aspire-orchestration-discovery` | **Date**: 2026-03-19  
**Spec**: [spec.md](spec.md)

> This feature introduces no application-layer data model or persistence. The entities below are **infrastructure conceptual entities** that describe the orchestration topology. They map directly to Aspire SDK constructs in `WeatherAdvisor.AppHost`.

---

## Entities

### Component Definition

Represents a locally runnable application unit registered with the Aspire AppHost.

| Field | Type | Description |
|-------|------|-------------|
| `name` | `string` | Resource identifier used by Aspire for naming, env var derivation, and dashboard display |
| `startupReference` | `IResourceBuilder<T>` | Aspire resource builder — either `ProjectResource` (for .NET projects) or `ViteAppResource` (for Vite/React dev servers) |
| `dependencies` | `IResourceBuilder<T>[]` | Resources this component depends on, expressed via `.WithReference()` |
| `injectedEnvironment` | `(string key, EndpointReference value)[]` | Runtime environment variable overrides injected by AppHost |

**Instances in this feature**:

| Name | Type | Dependencies | Injected Env Vars |
|------|------|---------|-------------------|
| `"weatheradvisor-api"` | `ProjectResource` (WeatherAdvisor.Api) | — | None (user secrets loaded natively) |
| `"frontend"` | `ViteAppResource` (Vite/React dev server) | `weatheradvisor-api` | `VITE_API_BASE_URL` → API http endpoint |

---

### Service Registration

Represents the runtime-discoverable endpoint metadata that enables one component to locate another.

| Field | Type | Description |
|-------|------|-------------|
| `resourceName` | `string` | Name of the upstream resource (e.g., `"weatheradvisor-api"`) |
| `scheme` | `"http"` \| `"https"` | Transport scheme |
| `endpointUrl` | `string` | Dynamically allocated URL (e.g., `http://localhost:34521`) |
| `aspireSimplifiedEnvVar` | `string` | Env var injected by Aspire 13 to polyglot resources: `WEATHERADVISOR_API_HTTP` |
| `aspireLegacyEnvVar` | `string` | Legacy .NET format: `services__weatheradvisor-api__http__0` |
| `viteEnvVar` | `string` | Explicit env var for Vite TypeScript client: `VITE_API_BASE_URL` |

**Service registration in this feature**:

| Source | Target | Transport | Frontend var | Browser-visible |
|--------|--------|-----------|-------------|-----------------|
| `frontend` | `weatheradvisor-api` | http | `VITE_API_BASE_URL` | Yes (browser makes direct API calls) |

**State transitions**:
- `unallocated` → `allocated` (AppHost assigns dynamic port on startup)
- `allocated` → `restarted` (component process restart; AppHost reassigns endpoint; env var re-injected on next spawn)
- `allocated` → `unavailable` (component process exits unexpectedly; Aspire marks as Failed)

---

### Orchestration Session

Represents a single developer-initiated run context, started by `aspire run` or `dotnet run` in the AppHost project.

| Field | Type | Description |
|-------|------|-------------|
| `sessionId` | runtime-internal | Unique identifier for the session (managed internally by Aspire) |
| `startedAt` | `DateTimeOffset` | Timestamp when the AppHost began orchestration |
| `components` | `Component Definition[]` | All registered resources in this session |
| `state` | `starting` \| `running` \| `degraded` \| `stopped` | Aggregate session health |
| `dashboardUrl` | `string` | URL for the Aspire dashboard (dynamic; printed to terminal at startup) |

**Lifecycle**:
1. Developer runs `aspire run` (or `dotnet run` in AppHost project) → session created
2. AppHost starts all registered components in dependency order
3. Aspire dashboard becomes available (dynamic port)
4. Developer stops session (`Ctrl+C`) → all child processes are terminated
5. Session ends; all dynamic port allocations are released

---

## Validation Rules

| Entity | Rule |
|--------|------|
| Component Definition | `name` must be unique within the AppHost; Aspire enforces this at build |
| Component Definition | ViteApp resource must NOT have a duplicate `.WithHttpEndpoint()` call |
| Service Registration | `VITE_API_BASE_URL` is only valid during `npm run dev`; not suitable for static production builds |
| Orchestration Session | User secrets must be populated in the API project's secrets store before the session starts, or the API will start with missing/empty configuration values |

---

## Notes

- Per Constitution §IV (No-Persistence Constraint): no database, cache, or file storage is introduced by this feature.
- All entities above are **ephemeral** — they exist only for the duration of an Orchestration Session and leave no durable state.
- The `WeatherAdvisor.AppHost` project acts as the sole implementation of all three entities; no new service or application layer code is introduced.
