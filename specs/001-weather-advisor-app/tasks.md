# Tasks: Weather Advisor

**Input**: Design documents from `/specs/001-weather-advisor-app/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/api.md ✅, quickstart.md ✅

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no incomplete dependencies)
- **[Story]**: User story this task belongs to (US1–US5)
- Exact file paths included in all task descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize the .NET 10 backend solution and Vite React frontend with required tooling and baseline configuration.

- [x] T001 Create .NET 10 solution with WeatherAdvisor.Api and WeatherAdvisor.Tests projects per plan.md structure in backend/
- [x] T002 Initialize Vite React TypeScript frontend project with Axios dependency in frontend/
- [x] T003 [P] Create backend appsettings.json with OpenMeteo GeocodingBaseUrl, ForecastBaseUrl, TimeoutSeconds (5), and logging levels in backend/WeatherAdvisor.Api/appsettings.json
- [x] T004 [P] Create frontend environment template in frontend/.env.example with VITE_API_BASE_URL=http://localhost:5000

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core shared infrastructure — configuration binding, Open-Meteo integration layer, domain models, and DI wiring — required by all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T005 Create OpenMeteoOptions configuration class binding GeocodingBaseUrl, ForecastBaseUrl, and TimeoutSeconds in backend/WeatherAdvisor.Api/Configuration/OpenMeteoOptions.cs
- [x] T006 [P] Create Open-Meteo deserialization models GeocodingResponse.cs and ForecastResponse.cs in backend/WeatherAdvisor.Api/Integration/Models/
- [x] T007 [P] Create domain request/response models: GetWeatherRequest.cs in backend/WeatherAdvisor.Api/Models/Requests/, WeatherResponse.cs and RecommendationResponse.cs in backend/WeatherAdvisor.Api/Models/Responses/, and ErrorResponse.cs in backend/WeatherAdvisor.Api/Models/
- [x] T008 Define IOpenMeteoClient interface with GetCoordinatesAsync and GetCurrentWeatherAsync method signatures in backend/WeatherAdvisor.Api/Integration/IOpenMeteoClient.cs
- [x] T009 Implement OpenMeteoClient using IHttpClientFactory with 5-second timeout enforcement for geocoding (R-001) and forecast (R-002) calls in backend/WeatherAdvisor.Api/Integration/OpenMeteoClient.cs
- [x] T010 Configure Program.cs with DI registrations for IOpenMeteoClient, IWeatherService, IActivityAdvisorService; HttpClientFactory named clients; CORS policy for localhost:5173; and structured logging in backend/WeatherAdvisor.Api/Program.cs
- [x] T011 [P] Define TypeScript types (WeatherResponse, RecommendationResponse, ErrorResponse, ActivityType enum) aligned with contracts/api.md in frontend/src/types/models.ts
- [x] T012 Implement weatherApiClient.ts with fetchWeather(city: string) and fetchRecommendation(weather, activity) functions using Axios and VITE_API_BASE_URL in frontend/src/services/weatherApiClient.ts

**Checkpoint**: Backend infrastructure and frontend API client ready — user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 — Retrieve City Weather (Priority: P1) 🎯 MVP

**Goal**: Users enter a city name and the system displays current temperature (°C), wind speed (km/h), precipitation probability (%), and condition label (Clear/Cloudy/Rainy/Snowy/Stormy).

**Independent Test**: Enter "London" → weather card displays all four weather fields. Enter "xyznotacity" → "City not found" message, no partial data. Disable backend → "Weather data is currently unavailable." message appears without crashing.

- [x] T013 [US1] Define IWeatherService interface with GetWeatherAsync(string city) returning WeatherResponse in backend/WeatherAdvisor.Api/Services/IWeatherService.cs
- [x] T014 [US1] Implement WeatherService orchestrating geocoding lookup → forecast fetch → WMO code-to-label mapping per data-model.md §3, classifying Open-Meteo failures to typed exceptions in backend/WeatherAdvisor.Api/Services/WeatherService.cs
- [x] T015 [US1] Implement WeatherController with GET /weather endpoint: model validation, address (city name) normalization, service call, and error-code mapping (400/404/503/504) to ErrorResponse envelope per contracts/api.md in backend/WeatherAdvisor.Api/Controllers/WeatherController.cs
- [x] T016 [P] [US1] Write WeatherService unit tests covering successful fetch, CITY_NOT_FOUND, WEATHER_SERVICE_TIMEOUT, WEATHER_SERVICE_UNAVAILABLE, all WMO label mappings, and input normalization (verify that mixed-case city names and city names with surrounding whitespace return the same result as the trimmed, normalized form — per US1/S2) in backend/WeatherAdvisor.Tests/Services/WeatherServiceTests.cs
- [x] T017 [P] [US1] Create CitySearch component with text input, submit handler, loading spinner, and error/empty state display in frontend/src/components/CitySearch.tsx
- [x] T018 [P] [US1] Create WeatherCard component displaying resolved city name, temperature (°C), wind speed (km/h), precipitation probability (%), and condition label in frontend/src/components/WeatherCard.tsx
- [x] T019 [US1] Implement useWeather hook managing weather fetch lifecycle (idle → loading → success/error) and invoking weatherApiClient.fetchWeather in frontend/src/hooks/useWeather.ts
- [x] T020 [US1] Compose HomePage with CitySearch and conditional WeatherCard rendering based on weather state (idle/loading/success/error) in frontend/src/pages/HomePage.tsx

**Checkpoint**: User Story 1 fully functional — city weather retrieval with all error states works end-to-end independently.

---

## Phase 4: User Story 2 — Get Activity Recommendation (Priority: P1) 🎯 MVP

**Goal**: Users select an outdoor activity (Running, Cycling, Picnic, Walking) and receive a verdict (Suitable/Caution/Not Recommended/Unknown) with a human-readable explanation referencing the causal weather factor.

**Independent Test**: Load weather for any city → select "Cycling" with wind > 40 km/h → "Not Recommended" with wind explanation. Load weather with WMO code 95 → select any activity → "Not Recommended" with extreme weather message. Missing weather field → "Unknown" verdict without crash.

- [ ] T021 [US2] Define IActivityAdvisorService interface with Evaluate(WeatherConditions conditions, ActivityType activity): Recommendation method in backend/WeatherAdvisor.Api/Services/IActivityAdvisorService.cs
- [ ] T022 [US2] Implement ActivityAdvisorService with threshold constants (data-model.md §2), global extreme-severity override rule, and per-activity ordered evaluation rules with value-interpolated explanation templates per data-model.md §4 in backend/WeatherAdvisor.Api/Services/ActivityAdvisorService.cs
- [ ] T023 [US2] Add POST /recommendation endpoint to WeatherController: deserialize request body, validate activity enum (422/UNSUPPORTED_ACTIVITY for unknown values), invoke IActivityAdvisorService, return RecommendationResponse per contracts/api.md in backend/WeatherAdvisor.Api/Controllers/WeatherController.cs
- [ ] T024 [P] [US2] Write ActivityAdvisorService unit tests for all four activities across Suitable, Caution, NotRecommended verdicts, extreme-severity override, and Unknown fallback for missing data in backend/WeatherAdvisor.Tests/Services/ActivityAdvisorServiceTests.cs
- [ ] T025 [P] [US2] Create ActivitySelector component presenting Running, Cycling, Picnic, Walking options with disabled state when no weather data is loaded in frontend/src/components/ActivitySelector.tsx
- [ ] T026 [P] [US2] Create RecommendationCard component displaying verdict label with visual differentiation, explanation text, and loading/error states in frontend/src/components/RecommendationCard.tsx
- [ ] T027 [US2] Extend useWeather hook to manage selected activity state and call weatherApiClient.fetchRecommendation using cached weather data on demand in frontend/src/hooks/useWeather.ts
- [ ] T028 [US2] Integrate ActivitySelector and RecommendationCard into HomePage below WeatherCard with conditional rendering (only shown after weather loads) in frontend/src/pages/HomePage.tsx

**Checkpoint**: Full MVP complete — city lookup → weather display → activity selection → recommendation with explanation works end-to-end.

---

## Phase 5: User Story 3 — Understand the Decision (Priority: P2)

**Goal**: Every recommendation explanation references at least one specific weather factor with its actual numeric value so users understand why the verdict was reached.

**Independent Test**: For every verdict tier (Suitable/Caution/NotRecommended) across all four activities, the explanation string displayed contains the actual interpolated value — e.g., "Rain probability is 72%", "Wind speed is 45 km/h" — not a generic message.

- [ ] T029 [P] [US3] Extend ActivityAdvisorService unit tests to assert each explanation string contains the actual interpolated weather value for all verdict/activity combinations (e.g., `Contains("72%")` when precipitationProbabilityPct = 72) in backend/WeatherAdvisor.Tests/Services/ActivityAdvisorServiceTests.cs
- [ ] T030 [P] [US3] Write Vitest component test for RecommendationCard verifying verdict label and explanation text prop are rendered visibly in frontend/tests/components/RecommendationCard.test.tsx

**Checkpoint**: Explanation content verified — each recommendation meaningfully cites the causal weather factor with actual values, confirmed by tests.

---

## Phase 6: User Story 4 — Switch Activities (Priority: P2)

**Goal**: Selecting a different activity instantly re-evaluates the cached weather and updates the recommendation with no page reload and no re-fetch of weather data.

**Independent Test**: Load weather for a city → select "Running" → view verdict → select "Cycling" → recommendation card updates immediately; browser network tab shows no new GET /weather request and no page navigation.

- [ ] T031 [US4] Update useWeather hook with useEffect to auto-trigger fetchRecommendation whenever selectedActivity changes while weatherData is non-null, without clearing weatherData in frontend/src/hooks/useWeather.ts
- [ ] T032 [P] [US4] Write Vitest integration test verifying that switching selectedActivity calls fetchRecommendation once without calling fetchWeather again in frontend/tests/components/HomePage.test.tsx

**Checkpoint**: Activity switching reactive — recommendation updates immediately on selection, weather data preserved, confirmed by integration test.

---

## Phase 7: User Story 5 — Handle Input and System Errors (Priority: P3)

**Goal**: All invalid inputs and system failures produce clear, actionable user messages with no crashes, no partial data, and no exposed internal errors.

**Independent Test**: Non-existent city → "City not found" with re-entry opportunity. Simulated API timeout → "retry" prompt. POST /recommendation with unsupported activity → HTTP 422 UNSUPPORTED_ACTIVITY. Unhandled backend exception → HTTP 500 with INTERNAL_ERROR code, no stack trace in response body.

- [ ] T033 [US5] Implement comprehensive error state management in useWeather hook: map each API error code to a typed user message, clear weatherData and recommendation on CITY_NOT_FOUND per FR-008 in frontend/src/hooks/useWeather.ts
- [ ] T034 [P] [US5] Add error message display to CitySearch component for CITY_NOT_FOUND (re-entry prompt), WEATHER_SERVICE_TIMEOUT (retry prompt), and WEATHER_SERVICE_UNAVAILABLE messages in frontend/src/components/CitySearch.tsx
- [ ] T035 [P] [US5] Register global exception handler middleware in Program.cs suppressing stack traces and returning INTERNAL_ERROR ErrorResponse for all unhandled exceptions in backend/WeatherAdvisor.Api/Program.cs
- [ ] T036 [P] [US5] Write OpenMeteoClient unit tests for 5-second timeout enforcement and non-success HTTP response classification (503 → WEATHER_SERVICE_UNAVAILABLE, timeout → WEATHER_SERVICE_TIMEOUT) in backend/WeatherAdvisor.Tests/Integration/OpenMeteoClientTests.cs

**Checkpoint**: All error scenarios handled — every failure mode produces a user-readable message; global exception handler prevents internal error exposure.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Developer-experience configuration, environment file completeness, and full end-to-end quickstart validation.

- [ ] T037 [P] Create appsettings.Development.example.json with debug-level logging override for WeatherAdvisor namespace in backend/WeatherAdvisor.Api/appsettings.Development.example.json
- [ ] T038 [P] Ensure weatherApiClient.ts reads VITE_API_BASE_URL from import.meta.env with a localhost:5000 fallback for local development in frontend/src/services/weatherApiClient.ts
- [ ] T039 Run complete end-to-end quickstart validation per quickstart.md: backend starts on :5000 (`dotnet run`), frontend starts on :5173 (`npm run dev`), full user journey (city entry → weather → activity → recommendation) works in browser

---

## Dependencies & Execution Order

### Phase Dependencies

| Phase | Depends On | Notes |
|-------|------------|-------|
| Setup (Phase 1) | None | Start immediately |
| Foundational (Phase 2) | Phase 1 | Blocks ALL user stories |
| US1 (Phase 3) | Phase 2 | No story dependencies |
| US2 (Phase 4) | Phase 2, US1 | Adds POST endpoint to WeatherController created in US1 |
| US3 (Phase 5) | US2 | Extends ActivityAdvisorService tests and RecommendationCard |
| US4 (Phase 6) | US2 | Extends useWeather hook built in US2 |
| US5 (Phase 7) | US1, US2 | Polishes error handling across layers from both stories |
| Polish (Final) | All stories | End-to-end validation only after all stories complete |

### User Story Dependencies

- **US1 (P1)**: Fully independent after Foundational phase — no story dependencies
- **US2 (P1)**: Depends on US1 (POST /recommendation added to WeatherController from T015); independently testable for recommendation logic
- **US3 (P2)**: Depends on US2 — extends ActivityAdvisorService tests (T029 extends T024's file) and adds component test; no new runtime paths
- **US4 (P2)**: Depends on US2 — adds reactive useEffect to useWeather hook (T031 extends T027's file); frontend-only change
- **US5 (P3)**: Depends on US1 and US2 — polishes error paths established in T014/T015 and T022/T023

### Within Each User Story

- Backend interface before implementation (e.g., T013 → T014 → T015)
- Services before controllers (e.g., T014 complete before T015 adds controller logic)
- Hook before components that consume it (e.g., T019 before T020)
- Implementation complete before test tasks in the same phase

### Parallel Opportunities Per Story

**US1 (Phase 3)** — after T015 is complete:
```
T015 → [T016, T017, T018] (parallel) → T019 → T020
```

**US2 (Phase 4)** — after T022 and T023 are complete:
```
T021 → T022 → T023 → [T024, T025, T026] (parallel) → T027 → T028
```

**US3 (Phase 5)**:
```
[T029, T030] (fully parallel)
```

**US4 (Phase 6)**:
```
T031 → T032
```

**US5 (Phase 7)**:
```
T033 → T034
[T035, T036] (parallel, different files)
```

---

## Implementation Strategy

### MVP Scope (Deliver First)

Complete **Phases 1–4** (T001–T028) to deliver the full core user journey:

> City name entry → Weather display → Activity selection → Recommendation with explanation

This covers both P1 user stories (US1 + US2) in 28 tasks.

### Incremental Delivery Milestones

| Milestone | Tasks | Deliverable |
|-----------|-------|-------------|
| Project shells | T001–T004 | Backend solution + frontend Vite app running locally |
| Infrastructure | T005–T012 | Open-Meteo client wired, TypeScript types defined, API client ready |
| MVP backend | T013–T015, T021–T023 | Both `/weather` and `/recommendation` endpoints functional |
| MVP frontend | T016–T020, T024–T028 | Complete UI: city search → weather → activity → recommendation |
| Explanation quality | T029–T030 | Explanation content verified by tests |
| Reactive UX | T031–T032 | Activity switching instant, test coverage confirmed |
| Error polish | T033–T036 | All error messages clear; global handler in place |
| Ship-ready | T037–T039 | Developer config, env files, quickstart validated |

### Suggested Parallel Team Allocation (if 2 developers)

- **Developer A**: Backend pipeline — T001, T005–T010, T013–T016, T021–T024, T029, T035–T036
- **Developer B**: Frontend — T002, T011–T012, T017–T020, T025–T028, T030–T032, T033–T034

Both developers share Phase 1 setup tasks (T001–T004) before diverging.
