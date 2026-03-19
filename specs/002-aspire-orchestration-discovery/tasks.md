# Tasks: Unified Local Orchestration and Discovery

**Input**: Design documents from `/specs/002-aspire-orchestration-discovery/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅  
**Tests**: No test tasks — the feature specification explicitly excludes new test projects (`plan.md`: "no new test projects for this feature").  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- `.NET projects`: `backend/WeatherAdvisor.*/`
- `New AppHost project`: `backend/WeatherAdvisor.AppHost/`
- `Modified API project`: `backend/WeatherAdvisor.Api/`
- `Solution file`: `backend/WeatherAdvisor.slnx`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the AppHost project file and register it in the solution.

- [ ] T001 Create `backend/WeatherAdvisor.AppHost/WeatherAdvisor.AppHost.csproj` using `Sdk="Aspire.AppHost.Sdk/13.1.3"`, `<OutputType>Exe</OutputType>`, `<TargetFramework>net10.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, a newly generated GUID as `<UserSecretsId>`, a `<ProjectReference Include="..\WeatherAdvisor.Api\WeatherAdvisor.Api.csproj" />`, and `<PackageReference Include="Aspire.Hosting.JavaScript" Version="13.1.3" />`
- [ ] T002 [P] Add `<Project Path="WeatherAdvisor.AppHost/WeatherAdvisor.AppHost.csproj" />` entry to `backend/WeatherAdvisor.slnx` alongside the existing Api and Tests project entries

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Ensure the API project has a `<UserSecretsId>` element so that `ASPNETCORE_ENVIRONMENT=Development` triggers user-secrets loading automatically when Aspire starts the API process.

**⚠️ CRITICAL**: Must be complete before User Story 1 can be validated end-to-end.

- [ ] T003 Check `backend/WeatherAdvisor.Api/WeatherAdvisor.Api.csproj` for a `<UserSecretsId>` element; if absent, run `dotnet user-secrets init` from `backend/WeatherAdvisor.Api/` to generate and insert it (per orchestration contract: Aspire sets `ASPNETCORE_ENVIRONMENT=Development`, which causes ASP.NET Core to load user secrets only if `<UserSecretsId>` is present)

**Checkpoint**: Foundation ready — user story implementation can begin

---

## Phase 3: User Story 1 — Start Full App Stack in One Command (Priority: P1) 🎯 MVP

**Goal**: A single `aspire run --project backend/WeatherAdvisor.AppHost` (or `dotnet run` from the AppHost directory) starts both `WeatherAdvisor.Api` and the Vite/React dev server without any manual per-component startup steps.

**Independent Test**: Run `aspire run --project backend/WeatherAdvisor.AppHost` from the repository root with prerequisites installed; confirm both `weatheradvisor-api` and `frontend` reach Running state as shown in the terminal output and Aspire dashboard.

### Implementation for User Story 1

- [ ] T004 [US1] Create `backend/WeatherAdvisor.AppHost/Program.cs` with the complete orchestration entry point: `var builder = DistributedApplication.CreateBuilder(args);` → register API via `var api = builder.AddProject<Projects.WeatherAdvisor_Api>("weatheradvisor-api");` → register frontend via `builder.AddViteApp("frontend", "../frontend").WithReference(api).WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"));` → `builder.Build().Run();` — do NOT add `.WithHttpEndpoint()` to the ViteApp resource (it registers one automatically; adding another causes a duplicate endpoint error per research Decision 3)

**Checkpoint**: `aspire run --project backend/WeatherAdvisor.AppHost` starts both components — User Story 1 is fully functional and independently testable

---

## Phase 4: User Story 2 — Resolve Internal Service Endpoints Automatically (Priority: P2)

**Goal**: The frontend receives the API's dynamically assigned base URL as `VITE_API_BASE_URL` (injected by AppHost via `.WithEnvironment`), and the API's CORS policy permits cross-origin requests from the frontend's dynamically assigned port — so browser-to-API calls succeed without any hardcoded `localhost` port in source files.

**Independent Test**: Start the stack in a fresh environment with no pre-existing port configuration; open the frontend at the URL printed by Aspire; submit a weather query — the API call succeeds using the injected URL, confirming no hardcoded port is required and CORS is not blocking the response.

### Implementation for User Story 2

- [ ] T005 [US2] Update the CORS policy in `backend/WeatherAdvisor.Api/Program.cs`: replace the existing single-branch `WithOrigins("http://localhost:5173")` configuration with a conditional: add `if (builder.Environment.IsDevelopment()) { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); }` and move the existing `WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()` into an `else` branch — exact target state is documented in `specs/002-aspire-orchestration-discovery/plan.md` under "CORS update in WeatherAdvisor.Api/Program.cs (target state)"

**Checkpoint**: User Story 2 fully functional — browser submits requests to the `VITE_API_BASE_URL` dynamic URL and CORS no longer blocks responses; User Stories 1 and 2 are both independently testable

---

## Phase 5: User Story 3 — Diagnose Local Environment Health Quickly (Priority: P3)

**Goal**: The Aspire dashboard (automatically included in the AppHost runtime) surfaces per-component state so a developer can identify a failing component within 1 minute without scanning disconnected terminal windows.

**Independent Test**: With the stack running, open the Aspire dashboard URL printed to the terminal; confirm `weatheradvisor-api` and `frontend` appear with their state; confirm a failed component (e.g., simulate by terminating one process) is reflected as Failed/Stopped in the dashboard within 1 minute.

### Implementation for User Story 3

- [ ] T006 [US3] Validate Aspire dashboard accessibility: run `aspire run --project backend/WeatherAdvisor.AppHost`, confirm the dashboard URL (format `https://localhost:{port}/login?t={token}`) is printed to the terminal, open the URL in a browser, and confirm both `weatheradvisor-api` and `frontend` are listed as **Running** — no code changes are required because the Aspire dashboard is built into the AppHost runtime (SC-004 acceptance criteria are satisfied by this built-in capability)

**Checkpoint**: All three user stories are independently functional — the full local stack runs, components are observable via the Aspire dashboard, and any failure is identifiable within 1 minute

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Regression check and end-to-end quickstart validation across the full feature.

- [ ] T007 [P] Run `dotnet test` from `backend/` to confirm no regressions introduced by the CORS policy update in `backend/WeatherAdvisor.Api/Program.cs` — all existing tests in `WeatherAdvisor.Tests/` must pass
- [ ] T009 [P] Verify individual component startup still works (FR-007): run `dotnet run --project backend/WeatherAdvisor.Api/WeatherAdvisor.Api.csproj` and confirm the API starts on its standalone port; then run `npm run dev` from `frontend/` and confirm the Vite dev server starts independently — both MUST succeed without the AppHost
- [ ] T008 Run full quickstart validation per `specs/002-aspire-orchestration-discovery/quickstart.md` steps 3–4: start the stack with `aspire run --project backend/WeatherAdvisor.AppHost`, confirm all components reach Running state in the terminal, open the Aspire dashboard, query the API via the dashboard-printed port using the PowerShell snippet from step 4, and verify end-to-end weather lookup succeeds from the browser (confirms SC-001 through SC-004)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; T001 and T002 can run in parallel
- **Foundational (Phase 2)**: T003 can run in parallel with Phase 1 at the file level (modifies `WeatherAdvisor.Api.csproj`, not the new AppHost files); treat it as a prerequisite gate before US1 validation
- **User Story 1 (Phase 3)**: Depends on T001 (AppHost `.csproj` must exist before `Program.cs` is written and `dotnet build` resolves `Projects.WeatherAdvisor_Api`) and T003 (user secrets prerequisite)
- **User Story 2 (Phase 4)**: T005 modifies `WeatherAdvisor.Api/Program.cs` — different file from T004; can be implemented in parallel at the file level, but validate together with US1
- **User Story 3 (Phase 5)**: T006 is a validation task requiring both T004 (AppHost running) and T005 (CORS working) to pass acceptance criteria meaningfully
- **Polish (Phase 6)**: T007, T009, and T008 can run in parallel after all user stories are complete

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 1 + Phase 2 — no dependency on US2 or US3
- **User Story 2 (P2)**: T005 is file-independent of T004 but logically depends on US1 for end-to-end validation
- **User Story 3 (P3)**: Validation-only; depends on US1 + US2 being complete for meaningful acceptance testing

### Within Each Phase

- Tests are NOT included (not requested; `plan.md` explicitly states no new test projects)
- T001 → T004 (AppHost `.csproj` must exist before `Program.cs` builds)
- T005 is file-independent of T004 (different project/file)

---

## Parallel Execution Examples

### Phase 1: Setup

```bash
# T001 and T002 can run in parallel (different files):
# Agent A: Create backend/WeatherAdvisor.AppHost/WeatherAdvisor.AppHost.csproj
# Agent B: Add AppHost entry to backend/WeatherAdvisor.slnx
```

### Phase 3 + Phase 4 (US1 + US2 overlap)

```bash
# T004 and T005 can run in parallel (different files:
#   backend/WeatherAdvisor.AppHost/Program.cs vs.
#   backend/WeatherAdvisor.Api/Program.cs):
# Agent A: Create AppHost Program.cs (T004)
# Agent B: Update API CORS policy (T005)
```

### Phase 6: Polish

```bash
# T007, T009, and T008 can run in parallel:
# Agent A: dotnet test backend/
# Agent B: npm run dev / dotnet run standalone check (FR-007)
# Agent C: aspire run + quickstart manual validation
```

---

## Implementation Strategy

### MVP Scope (Recommended first increment)

**MVP = User Story 1 only** (Phase 1 → Phase 2 → Phase 3):

- T001, T002 — create AppHost project and register in solution
- T003 — verify user secrets prerequisite
- T004 [US1] — AppHost `Program.cs` (single-command startup)

At the MVP checkpoint: `aspire run` starts both components. The API is reachable. The frontend Vite dev server starts. Cross-origin requests from the browser may fail (CORS blocks the dynamic port) until US2 (T005) is added.

### Full Feature (add US2 → US3 → Polish after MVP)

- T005 [US2] — CORS update to allow dynamic frontend port
- T006 [US3] — Dashboard validation (no code changes required)
- T007, T009, T008 — Regression tests, standalone startup check (FR-007), and quickstart sign-off

---

## Summary

| Phase | Tasks | Story | Key Deliverable |
|-------|-------|-------|-----------------|
| 1 — Setup | T001, T002 | — | AppHost project scaffold + solution registration |
| 2 — Foundational | T003 | — | API project has `<UserSecretsId>` for secrets forwarding |
| 3 — US1 (P1) | T004 | US1 | `aspire run` starts API + frontend (single command) |
| 4 — US2 (P2) | T005 | US2 | CORS allows dynamic frontend port (auto-discovery works) |
| 5 — US3 (P3) | T006 | US3 | Aspire dashboard surfaces component health (validation) |
| 6 — Polish | T007, T009, T008 | — | Regression tests pass; standalone startup verified (FR-007); quickstart validated end-to-end |

**Total tasks**: 8  
**US1 tasks**: 1 (T004)  
**US2 tasks**: 1 (T005)  
**US3 tasks**: 1 (T006)  
**Parallel opportunities**: Phase 1 (T001‖T002), Phase 3+4 (T004‖T005), Phase 6 (T007‖T009‖T008)  
**Suggested MVP**: Phase 1 → Phase 2 → Phase 3 (T001–T004)  
**Format validation**: All tasks follow `- [ ] TID [P?] [Story?] Description with file path` ✅
