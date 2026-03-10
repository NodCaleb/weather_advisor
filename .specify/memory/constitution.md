<!--
SYNC IMPACT REPORT
==================
Version change: 0.0.0 → 1.0.0 (MAJOR — initial constitution, all principles new)

Added sections:
  - Preamble
  - I.   Specification-First Delivery
  - II.  Clear Separation of Concerns
  - III. API-First Contract Discipline
  - IV.  No-Persistence Constraint
  - V.   Security and Secrets Management
  - VI.  Simplicity and Maintainability
  - VII. Testability and Verification
  - VIII.Reliability and Failure Handling
  - IX.  Observability for Development
  - X.   Developer Experience
  - XI.  Frontend Principles
  - XII. Backend Principles
  - Governance

Templates reviewed:
  ✅ .specify/templates/plan-template.md   — Constitution Check gate is dynamically filled by
                                             /speckit.plan; no structural change needed.
  ✅ .specify/templates/spec-template.md   — Existing sections cover spec-first, acceptance
                                             criteria, and FR/SC patterns; no change needed.
  ✅ .specify/templates/tasks-template.md  — Phase structure, web-app path conventions, and
                                             optional testing model align with principles.

Follow-up TODOs: none — all fields resolved.
-->

# Weather Advisor Constitution

## Preamble

This constitution governs the architecture, engineering practices, and delivery process
for the Weather Advisor application — a web application that advises users on how to dress
for current weather conditions in a specified city. It comprises a .NET backend that
orchestrates public weather API integrations and a React frontend that presents
recommendations to users.

This document is the supreme governing authority for all specifications, plans, and
implementation decisions on this project. Every feature specification, implementation
plan, and task list MUST demonstrably comply with the principles below. Where a
feature or technical decision appears to conflict with this constitution, the
constitution takes precedence until formally amended through the process defined in
the Governance section.

This constitution is optimised for practical use within a Spec Kit specification-first
development workflow.

---

## Core Principles

### I. Specification-First Delivery

All work MUST begin from an explicit, approved specification before any implementation
activity starts. Requirements, architecture, interfaces, and acceptance criteria MUST be
defined and recorded in a spec document before coding begins.

- Specs are the single source of truth for implementation decisions.
- Implementation changes MUST trace back to a corresponding approved spec change.
- Code that cannot be traced to a spec requirement MUST be considered out of scope.
- The `/speckit.specify`, `/speckit.plan`, and `/speckit.tasks` workflow MUST be followed
  for every feature, however small.

**Rationale**: Prevents scope creep, undocumented decisions, and implementation drift in a
project driven by iterative delivery.

### II. Clear Separation of Concerns

Backend and frontend MUST be independently structured and loosely coupled at all times.

- **Backend responsibilities**: HTTP API exposure, business logic, validation, orchestration
  of external public API calls, and error normalization.
- **Frontend responsibilities**: Presentation, user interaction, local UI state management,
  and consumption of backend HTTP APIs exclusively.
- The frontend MUST NOT call external public APIs directly. All external data MUST flow
  through the backend.
- Shared contracts (request/response shapes, error envelopes) MUST be explicitly defined
  and treated as versioned interfaces.

**Rationale**: Keeps each tier independently understandable, testable, and replaceable
without cascading changes across the system.

### III. API-First Contract Discipline

The backend MUST expose well-defined HTTP API contracts as the sole integration surface
for the frontend.

- All API contracts (endpoints, request models, response DTOs, error envelopes) MUST be
  documented in the feature spec before implementation.
- External public API integrations MUST be abstracted behind internal service interfaces;
  no controller or application-layer code may call an external HTTP client directly.
- DTOs, request/response models, and error contracts MUST be explicit, consistent, and
  agreed upon in the spec phase.
- The system MUST be designed to tolerate failures or schema changes in external APIs
  without propagating unhandled exceptions to the frontend.

**Rationale**: Decoupled contracts enable independent frontend/backend development and
protect the system from third-party API instability.

### IV. No-Persistence Constraint (NON-NEGOTIABLE)

The architecture MUST assume no database, file system storage, or any other form of
long-term or persistent storage.

- Introducing a database, cache store, file-based persistence, or any equivalent
  storage mechanism is **PROHIBITED** unless this constitution is formally amended and
  the project scope is explicitly updated.
- Application logic MUST prefer stateless, request-scoped processing.
- Any in-memory state (e.g., caching within a single request lifecycle) MAY be used only
  where explicitly justified in a spec and MUST be documented.
- Behaviour under process restart MUST be predictable: no data loss scenarios are possible
  because no data is retained.

**Rationale**: Eliminates an entire category of infrastructure complexity and keeps the
system suitable for a demo/prototype without hidden storage dependencies.

### V. Security and Secrets Management

Secrets MUST be managed safely at all times, from development through deployment.

- API keys, tokens, credentials, and any sensitive configuration values MUST be supplied
  exclusively through environment variables or a secure runtime configuration mechanism
  (e.g., .NET user secrets for local development).
- Secrets MUST NOT be committed to source control under any circumstances. This applies to
  `.env` files, `appsettings.json` default values, test fixtures, and documentation.
- Configuration MUST clearly distinguish between safe non-secret settings (committed to
  source control) and secret values (injected at runtime).
- Logging MUST NOT emit secret values, API keys, tokens, or any credentials. Log entries
  containing request/response payloads MUST be reviewed to ensure no secrets are included.

**Rationale**: Prevents credential exposure and enforces a zero-secrets-in-code policy
aligned with standard secure software practices.

### VI. Simplicity and Maintainability

The simplest solution that meets the specification MUST be preferred over clever,
abstract, or over-engineered alternatives.

- Complexity MUST be justified in the feature plan; unjustified complexity MUST be removed
  or deferred.
- Infrastructure and library dependencies MUST be minimized; each dependency MUST have a
  clear, documented purpose.
- Patterns and abstractions appropriate for a small-team demo/prototype context SHOULD be
  chosen over enterprise-scale patterns unless a spec explicitly requires otherwise.
- The architecture MUST remain easy for a small team to onboard, understand, and modify
  without specialist knowledge beyond standard .NET and React practices.

**Rationale**: A minimal, readable codebase is easier to extend, debug, and hand off than
a feature-rich but opaque one.

### VII. Testability and Verification

The system MUST be designed so that critical logic can be verified in isolation.

- Critical backend business logic and validation MUST be testable without a running HTTP
  server, external network, or side effects.
- External API integrations MUST be encapsulated behind interfaces or abstractions that
  can be replaced with test doubles (mocks, fakes, stubs) during testing.
- Frontend components and their API interactions SHOULD be testable using standard React
  testing tooling.
- Acceptance criteria defined in a spec MUST be verifiable; untestable acceptance criteria
  MUST be revised or removed from the spec.

**Rationale**: Verifiable code reduces regression risk and gives teams confidence to
iterate quickly.

### VIII. Reliability and Failure Handling

The system MUST handle external API failures gracefully without crashing or producing
undefined behaviour.

- Every integration point with an external public API MUST have an explicit error-handling
  strategy documented in the feature spec or plan.
- Timeout, rate-limit, network error, and malformed-response scenarios MUST be handled at
  the service boundary; they MUST NOT leak raw exceptions to the HTTP response layer.
- User-facing behaviour under degraded or unavailable external dependencies MUST be
  defined in the spec and implemented predictably (e.g., a clear error message rather than
  an unhandled 500).
- The backend MUST return structured, consistent error responses to the frontend regardless
  of the root cause of failure.

**Rationale**: Graceful degradation ensures a working demo experience even when third-party
services are unreliable or rate-limited.

---

## Development and Operations

### IX. Observability for Development

The application SHOULD include pragmatic, targeted logging to support development and
integration troubleshooting.

- Structured logging SHOULD be used in the .NET backend to enable filtering and
  correlation of integration-related events.
- Log entries MUST NOT contain secrets, tokens, or credentials (see Principle V).
- Logging verbosity SHOULD be configurable via standard .NET configuration (e.g., log
  level in `appsettings.Development.json`).
- Heavy production-grade observability tooling (distributed tracing, metrics pipelines,
  APM agents) MUST NOT be introduced unless a spec explicitly justifies the need.

**Rationale**: Sufficient visibility for a demo/prototype without adding infrastructure
overhead that obscures simplicity.

### X. Developer Experience

Local development setup MUST be straightforward and fully documented.

- Instructions for running the backend, frontend, and any required configuration MUST be
  maintained in the project README or a dedicated quickstart document.
- Environment variable requirements MUST be documented with a checked-in example file
  (e.g., `.env.example` or `appsettings.Development.example.json`) that contains no real
  secrets.
- Local setup MUST NOT require paid tooling, proprietary infrastructure, or manual
  steps that cannot be scripted or documented.
- The project structure MUST support iterative Spec Kit development: each feature SHOULD
  be independently specifiable, plannable, and implementable without rebuilding unrelated
  parts.

**Rationale**: A fast, low-friction developer loop is essential for productive spec-driven
iteration on a prototype project.

### XI. Frontend Principles

The React frontend MUST follow a clear, predictable structure.

- Component structure MUST reflect a consistent organisation (e.g., pages, components,
  services/hooks) defined in the project plan and maintained across features.
- State management MUST be predictable and scoped appropriately; global state SHOULD be
  minimised and justified in a spec when introduced.
- The frontend MUST consume backend APIs exclusively for all external data. Direct calls
  from the frontend to public APIs are PROHIBITED (see Principle II).
- Loading, empty, and error states MUST be handled explicitly in every UI component that
  performs an API call. Unhandled loading or error states MUST be treated as defects.

**Rationale**: Consistent UI behaviour and a strict data-flow boundary prevent
unpredictable UX and keep the frontend decoupled from external API concerns.

### XII. Backend Principles

The .NET backend MUST maintain clean boundaries between its structural layers.

- **Controllers/Endpoints**: MUST only handle HTTP concerns (routing, model binding,
  response serialisation). Business logic MUST NOT reside in controllers.
- **Application/Service layer**: MUST contain orchestration logic, validation, and
  coordination between domain logic and external integrations.
- **External integration layer**: MUST encapsulate all external HTTP client calls behind
  interfaces or typed service classes. No raw `HttpClient` calls SHOULD appear outside
  this layer.
- `HttpClient` configuration MUST use the .NET `IHttpClientFactory` pattern with named or
  typed clients. HttpClient instances MUST NOT be instantiated directly.
- Configuration MUST follow standard .NET `IOptions<T>` or `IConfiguration` patterns;
  settings classes MUST distinguish between safe and secret values.

**Rationale**: Clean layer separation enables unit testing at each boundary, reduces
coupling, and follows established .NET idioms that any .NET developer can navigate
without project-specific explanation.

---

## Governance

This constitution supersedes all informal practices, PR conventions, or verbal
agreements. It is binding on all specifications, plans, and implementation decisions
for the Weather Advisor project.

### Amendment Process

1. A proposed amendment MUST be described as a written change to this file, stating
   the principle or section affected, the proposed new text, and the justification.
2. The amendment MUST be reviewed and explicitly approved before it takes effect.
3. After approval, the `CONSTITUTION_VERSION` MUST be incremented following semantic
   versioning rules (see below), and `LAST_AMENDED_DATE` MUST be updated.
4. Any spec, plan, or task list created before the amendment MUST be reviewed for
   compliance with the new version; affected documents MUST be updated or explicitly
   marked as grandfathered with justification.

### Versioning Policy

- **MAJOR**: Removal or backward-incompatible redefinition of a principle (e.g., lifting
  the No-Persistence Constraint).
- **MINOR**: Addition of a new principle, section, or materially expanded guidance.
- **PATCH**: Clarifications, wording improvements, typo fixes, or non-semantic refinements.

### Compliance Review

- Every feature spec MUST include a constitution compliance check before implementation
  begins (enforced by `/speckit.plan` Constitution Check gate).
- Any implementation decision that deviates from this constitution MUST be surfaced
  in the plan's Complexity Tracking table with explicit justification.
- Code review SHOULD verify that no implementation introduces patterns prohibited by this
  constitution (e.g., direct external API calls from the frontend, committed secrets,
  persistent storage).

**Version**: 1.0.0 | **Ratified**: 2026-03-10 | **Last Amended**: 2026-03-10
