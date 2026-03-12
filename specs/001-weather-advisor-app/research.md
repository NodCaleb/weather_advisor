# Research: Weather Advisor

**Branch**: `001-weather-advisor-app` | **Date**: March 12, 2026  
**Phase**: 0 — Outline & Research

---

## R-001: Open-Meteo Geocoding API

**Question**: How do we resolve a user-entered city name to geographic coordinates for the forecast API?

**Decision**: Use the Open-Meteo Geocoding API with `count=1` to retrieve the top-matching result.

**Endpoint**:
```
GET https://geocoding-api.open-meteo.com/v1/search
    ?name={cityName}&count=1&language=en&format=json
```

**Response shape (successful)**:
```json
{
  "results": [
    {
      "id": 2950159,
      "name": "Berlin",
      "latitude": 52.52437,
      "longitude": 13.41053,
      "country_code": "DE",
      "country": "Deutschland",
      "admin1": "Berlin",
      "timezone": "Europe/Berlin"
    }
  ]
}
```

**City-not-found handling**: When no match exists, the `results` field is absent or is an empty array. The backend MUST treat both cases as "city not found" and return HTTP 404 with error code `CITY_NOT_FOUND`.

**Rationale**: First result from fuzzy-match geocoding is the most relevant for a plain city-name search. Using `count=1` eliminates the need to select among candidates.

**Alternatives considered**:
- Google Maps Geocoding API — requires paid API key; out of scope.
- Nominatim (OpenStreetMap) — free but has stricter rate limits and usage policy; Open-Meteo's own geocoder is simpler with no key required.

---

## R-002: Open-Meteo Forecast API — Current Weather Variables

**Question**: Which variables are available in the `current` section of the forecast API? Is `precipitation_probability` available directly?

**Decision**: Request all four required fields (`temperature_2m`, `wind_speed_10m`, `precipitation_probability`, `weather_code`) as `current` variables in a single request.

**Endpoint**:
```
GET https://api.open-meteo.com/v1/forecast
    ?latitude={lat}
    &longitude={lon}
    &current=temperature_2m,wind_speed_10m,precipitation_probability,weather_code
    &wind_speed_unit=kmh
    &timezone=auto
```

**Confirmed available `current` variables**:
| Variable | Type | Unit | Purpose |
|----------|------|------|---------|
| `temperature_2m` | Float | °C | Display + Picnic threshold check |
| `wind_speed_10m` | Float | km/h | Display + Running/Cycling/Walking threshold check |
| `precipitation_probability` | Integer | % | Display + Running/Picnic threshold check |
| `weather_code` | Integer | WMO code | Display label + extreme severity gate |

**Key finding**: Open-Meteo states "every weather variable available in hourly data is available as current condition as well." `precipitation_probability` is a valid hourly variable, therefore it can be requested as a `current` variable. The value represents the probability of precipitation (>0.1 mm) for the preceding 15-minute interval.

**Rationale**: Single round-trip call with `current` is simpler and more efficient than combining `current` + `hourly` requests and extracting the matching hour index.

**Alternatives considered**:
- Requesting `hourly=precipitation_probability&forecast_days=1` and extracting the current-hour index — valid fallback but unnecessarily complex when `current` supports the variable directly.

---

## R-003: WMO Weather Code Label Mapping

**Question**: How do WMO weather codes returned by Open-Meteo map to human-readable condition labels for display?

**Decision**: Map WMO codes to five display labels: `Clear`, `Cloudy`, `Rainy`, `Snowy`, `Stormy`.

**Mapping table**:
| Code(s) | Label | Notes |
|---------|-------|-------|
| 0 | Clear | Clear sky |
| 1 | Clear | Mainly clear |
| 2 | Cloudy | Partly cloudy |
| 3 | Cloudy | Overcast |
| 45, 48 | Cloudy | Fog |
| 51, 53, 55, 56, 57 | Rainy | Drizzle (light/moderate/dense/freezing) |
| 61, 63, 65, 66, 67 | Rainy | Rain (slight/moderate/heavy/freezing) |
| 71, 73, 75, 77 | Snowy | Snowfall / snow grains |
| 80, 81, 82, 85, 86 | Rainy | Rain showers / snow showers |
| 95, 96, 99 | Stormy | Thunderstorm (extreme severity gate) |
| Any unknown | Cloudy | Safe fallback for unmapped codes |

**Extreme severity gate**: WMO codes 95, 96, 99 trigger the override to `Not Recommended` for all activities regardless of wind or precipitation values.

**Rationale**: Five labels cover all user-comprehensible conditions without excessive granularity. Fog is mapped to Cloudy (visually similar, not a distinct severity tier for this app). The "any unknown" fallback satisfies FR-011 (missing/unexpected data falls back gracefully).

---

## R-004: .NET Version and Project Structure

**Question**: Which .NET version to use? Minimal API vs. controller-based?

**Decision**: .NET 10 LTS with ASP.NET Core Web API using attribute-routed controllers.

**Rationale**:
- .NET 10 is the current Long-Term Support release (supported until November 2028); appropriate for a demo/prototype that should remain buildable throughout its lifetime.
- Controller-based Web API directly maps to the "Controllers/Endpoints" layer referenced in Constitution Principle XII, and is the pattern any .NET developer can navigate without project-specific knowledge.
- Minimal API is simpler for trivial single-file scenarios but provides less structural clarity as the application grows across two controllers and multiple service layers.

**Alternatives considered**:
- .NET 9 (non-LTS, EOL May 2026) — already past end-of-life at time of writing; not viable.
- .NET 8 LTS — still supported but superseded by .NET 10 LTS as the current LTS release.
- Minimal API (top-level handlers) — valid for very small surfaces; controller pattern chosen for clearer alignment with Constitution XII layering.

---

## R-005: Frontend Build Tooling and Testing

**Question**: Which build tooling and test framework for the React TypeScript frontend?

**Decision**: Vite + React + TypeScript for build; Vitest + React Testing Library for tests.

**Rationale**:
- Vite is the de-facto standard for React/TypeScript projects in 2026 (Create React App is deprecated and unmaintained).
- Vitest shares Vite's configuration, has near-identical API to Jest, and runs significantly faster for unit/component tests.
- React Testing Library encourages testing user-visible behaviour rather than implementation details, aligning with the acceptance-scenario-driven spec style.

**Alternatives considered**:
- Create React App — deprecated; not viable.
- Jest + ts-jest — functional but requires separate config from Vite; Vitest is the idiomatic choice in a Vite project.

---

## R-006: Activity Suitability Decision Engine Design

**Question**: How should the recommendation engine be structured to satisfy FR-004, FR-005, FR-010, FR-011, and SC-004?

**Decision**: Implement a stateless `ActivityAdvisorService` with a single `Evaluate(WeatherConditions, ActivityType): Recommendation` method, using an ordered rule set per activity.

**Rule evaluation order** (applied top-to-bottom; first match wins):
1. **Extreme severity override** (all activities): WMO 95/96/99 OR wind_speed > 60 km/h → `Not Recommended` with extreme-weather message.
2. **Activity-specific blocking rules** → `Not Recommended` with factor-specific message.
3. **Activity-specific caution rules** → `Caution` with factor-specific message.
4. **Default** → `Suitable` with confirmation message.
5. **Missing data fallback**: if any required field is null/missing → `Unknown` (FR-011).

**Threshold constants** (from spec, defined once in a shared constants class):
| Threshold | Value |
|-----------|-------|
| `WindCautionKmh` | 21 |
| `WindBlockingKmh` | 41 |
| `WindExtremeKmh` | 61 |
| `PrecipCautionPct` | 31 |
| `PrecipBlockingPct` | 61 |
| `TempBlockingCelsius` | 10 |

**Rationale**: A single ordered-rule method per activity is easy to unit-test in isolation and trace back to spec thresholds. Each rule produces a specific explanation string referencing the causal weather factor (FR-006, SC-006).

**Alternatives considered**:
- Strategy pattern per activity — adds an abstraction layer with no benefit at this scale; Constitution VI requires the simplest justifiable solution.
- Rules-engine library — far beyond the complexity needed for four activities and fixed thresholds.

---

## R-007: Error Response Envelope

**Question**: How should the backend communicate errors to the frontend?

**Decision**: Use a consistent JSON error envelope for all non-2xx responses.

**Error envelope**:
```json
{
  "code": "CITY_NOT_FOUND",
  "message": "City not found"
}
```

**Defined error codes**:
| HTTP Status | Code | Trigger |
|-------------|------|---------|
| 404 | `CITY_NOT_FOUND` | Geocoding returns no results |
| 503 | `WEATHER_SERVICE_UNAVAILABLE` | Open-Meteo HTTP error or network failure |
| 504 | `WEATHER_SERVICE_TIMEOUT` | Open-Meteo response exceeds 5-second timeout |
| 422 | `UNSUPPORTED_ACTIVITY` | Activity value not in the supported enum |
| 500 | `INTERNAL_ERROR` | Unexpected exception (message suppressed) |

**Rationale**: A typed `code` field lets the frontend branch on error type without string-parsing the `message`. Constitution VIII requires structured, consistent error responses; this envelope satisfies that requirement with minimal overhead.

---

## R-008: Frontend–Backend Communication

**Question**: How does the React frontend discover and call the .NET backend?

**Decision**: Use an environment variable (`VITE_API_BASE_URL`) for the backend base URL, defaulting to `http://localhost:5000` in development. Axios is used as the HTTP client.

**Rationale**:
- `VITE_` prefix variables are natively supported by Vite and baked into the bundle at build time.
- Axios provides typed response handling, timeout configuration, and interceptors for consistent error extraction — a cleaner DX than raw `fetch` for a structured API client.
- No secrets are involved; the base URL is a non-secret configuration value.

**`.env.example`** (to be checked in):
```
VITE_API_BASE_URL=http://localhost:5000
```

**Alternatives considered**:
- Raw `fetch` — viable but requires more boilerplate for error handling and timeout management.
- React Query / TanStack Query — adds caching and background-refresh features not needed for a stateless demo.
