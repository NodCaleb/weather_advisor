# Feature Specification: Unified Local Orchestration and Discovery

**Feature Branch**: `002-aspire-orchestration-discovery`  
**Created**: 2026-03-17  
**Status**: Draft  
**Input**: User description: "Add Aspire orchestrator to allow developer to start all components at once. Implement service discovery to allow components to communicate without need to specify URLs explicitly"

## Clarifications

### Session 2026-03-19

- Q: Should the Vite/React frontend be included in the Aspire AppHost, or remain a manual/separate startup step? → A: Include frontend in AppHost via `AddNpmApp` (Vite dev server managed by Aspire)
- Q: How should local developer secrets (e.g. API keys) be supplied to Aspire-managed components? → A: User Secrets (`dotnet user-secrets`) forwarded automatically by Aspire to the API resource
- Q: How does the frontend resolve the backend API URL at runtime under Aspire? → A: Aspire injects backend URL as env var; Vite config maps it to `VITE_API_BASE_URL`
- Q: Should Aspire assign ports dynamically or use fixed declared ports in the AppHost? → A: Dynamic port assignment by Aspire (no fixed ports declared in AppHost)
- Q: Is CI/CD pipeline orchestration (e.g. Aspire test-host in CI) in scope for this feature? → A: Strictly local interactive developer use — CI/CD pipeline orchestration is out of scope

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start Full App Stack in One Command (Priority: P1)

As a developer, I want to start all required application components together from a single entry point so I can begin development quickly without manually coordinating multiple processes.

**Why this priority**: This is the highest-value workflow improvement because daily development is blocked until all components are running.

**Independent Test**: Can be fully tested by launching the orchestrated local environment from a clean workspace and confirming every required component becomes available without manual startup steps.

**Acceptance Scenarios**:

1. **Given** the repository is checked out with required local dependencies installed, **When** the developer starts the orchestrated environment, **Then** all required application components start successfully in one workflow.
2. **Given** one component fails during startup, **When** the developer starts the orchestrated environment, **Then** the developer receives a clear failure signal indicating which component did not start.

---

### User Story 2 - Resolve Internal Service Endpoints Automatically (Priority: P2)

As a developer, I want components to discover each other automatically so internal communication works without manually hardcoding or sharing endpoint URLs.

**Why this priority**: It reduces configuration errors and prevents endpoint drift between local environments.

**Independent Test**: Can be tested by running the application stack in a fresh environment where no explicit inter-service URLs are preconfigured and verifying internal requests succeed.

**Acceptance Scenarios**:

1. **Given** components are started through the orchestration workflow, **When** one component sends a request to another component, **Then** the request succeeds without manually configured target URLs.
2. **Given** a component instance is restarted with a new runtime endpoint, **When** dependent components communicate with it, **Then** communication continues without developer reconfiguration.

---

### User Story 3 - Diagnose Local Environment Health Quickly (Priority: P3)

As a developer, I want a single place to see startup and runtime health of components so I can quickly identify and fix local environment issues.

**Why this priority**: Observability improves troubleshooting speed but is secondary to startup and communication reliability.

**Independent Test**: Can be tested by intentionally causing one component to fail and confirming the developer can identify the failing component and reason without scanning multiple disconnected logs.

**Acceptance Scenarios**:

1. **Given** the local stack is running, **When** the developer inspects orchestration status, **Then** each component's state is visible (running, starting, failed, or stopped).
2. **Given** a component is unhealthy, **When** the developer checks orchestration diagnostics, **Then** the failing component is identifiable within 1 minute.

### Edge Cases

- A required component binary or dependency is missing at launch time.
- Two components have conflicting local resource bindings during startup. *(Resolved: Aspire uses dynamic port assignment by default — no fixed ports are declared in the AppHost — eliminating port conflicts between components.)*
- A downstream component becomes unavailable after initial successful startup.
- A component starts successfully but fails readiness checks.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a single developer-triggered workflow that starts all required application components for local development.
- **FR-002**: The system MUST define which components are part of the local development topology and ensure each is included in the startup workflow. The topology consists of exactly two components: the **WeatherAdvisor.Api** (.NET backend) and the **frontend** (Vite/React dev server), both managed within the Aspire `AppHost` — the frontend via `AddNpmApp`.
- **FR-003**: The system MUST expose clear startup status per component, including success and failure outcomes.
- **FR-004**: The system MUST enable automatic internal service discovery so components can communicate without manually configured inter-service URLs. Specifically, Aspire MUST inject the backend API's runtime URL into the frontend npm resource as an environment variable (following the Aspire `services__{name}__{scheme}__{index}` convention); the Vite configuration MUST expose this as `VITE_API_BASE_URL` so the frontend TypeScript client can consume it without any hardcoded `localhost` port.
- **FR-005**: The system MUST allow dependent components to continue resolving target components after target restarts or endpoint changes during local development sessions.
- **FR-006**: The system MUST surface actionable diagnostic information when startup fails or inter-component communication fails.
- **FR-007**: Developers MUST be able to run existing local workflows for individual components when full-stack orchestration is not needed.
- **FR-008**: The system MUST document the one-command startup workflow and expected local prerequisites, including required `dotnet user-secrets` entries that must be populated before first launch.
- **FR-009**: The system MUST NOT require any secret values to be committed to source control; all runtime secrets for local development MUST be supplied via .NET User Secrets (`dotnet user-secrets`), which Aspire forwards automatically to the API resource.

### Key Entities *(include if feature involves data)*

- **Component Definition**: A locally runnable application unit, including identifying name, startup command reference, dependency relationships, and health state.
- **Service Registration**: Runtime-discoverable identity and endpoint metadata that allows one component to locate another component without static URLs. For the frontend-to-backend path, this is realised by Aspire injecting the backend base URL as an environment variable into the Vite dev-server process, mapped to `VITE_API_BASE_URL`.
- **Orchestration Session**: A single developer-initiated run context that tracks participating components, startup sequence, current state, and diagnostics.

### Assumptions

- The feature applies to local development environments used by project contributors.
- The orchestrated topology includes the WeatherAdvisor.Api backend and the Vite/React frontend; the frontend dev server is managed by Aspire via `AddNpmApp`.
- Local developer secrets (e.g. Open-Meteo API key) are managed via .NET User Secrets scoped to `WeatherAdvisor.Api`; Aspire forwards these automatically — no secrets are committed to source control.
- Aspire assigns all component ports dynamically; no fixed ports are declared in the AppHost.
- Existing application components remain independently runnable.
- Local prerequisites (runtime and SDK requirements) are already managed by repository setup documentation.

### Out of Scope

- CI/CD pipeline orchestration: using Aspire's test host or `dotnet aspire run` in automated pipelines is explicitly out of scope for this feature.
- Production deployment or cloud hosting configuration.
- Monitoring, alerting, or observability tooling beyond Aspire's local dashboard.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 90% of developers can start the full local application stack within 5 minutes from a clean pull by following documented setup and a single startup workflow.
- **SC-002**: 95% of local startup attempts result in all required components reaching a running state without manual process-by-process startup.
- **SC-003**: 100% of internal component-to-component calls in supported local workflows succeed without developers entering explicit inter-service URLs.
- **SC-004**: During local failure simulations, developers can identify the failing component and failure reason in under 1 minute in at least 90% of test runs.
