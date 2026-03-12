# Data Model: Weather Advisor

**Branch**: `001-weather-advisor-app` | **Date**: March 12, 2026  
**Phase**: 1 — Design & Contracts  
**Source**: spec.md (Key Entities, Activity Suitability Thresholds) + research.md (R-001 – R-008)

---

## 1. Domain Entities

### 1.1 City (Input / Transient)

Represents the user's requested location. Resolved at request time via geocoding; never persisted.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `name` | `string` | Non-empty, max 100 chars; normalized (trimmed, case-insensitive lookup) | User-supplied input |
| `latitude` | `double` | –90.0 to 90.0 | Populated by geocoding (R-001) |
| `longitude` | `double` | –180.0 to 180.0 | Populated by geocoding (R-001) |
| `displayName` | `string` | As returned by Open-Meteo | Used in API response for unambiguous labeling |

**Validation rules**:
- Input city name MUST be trimmed and non-empty after trim.
- If geocoding returns no results, the request fails with `CITY_NOT_FOUND` (FR-008).

---

### 1.2 WeatherConditions (Value Object / Transient)

A snapshot of current atmospheric data for a resolved city. Sourced from Open-Meteo Forecast API (R-002). Never persisted.

| Field | Type | Unit | Nullable | Notes |
|-------|------|------|----------|-------|
| `temperatureCelsius` | `double` | °C | No | `temperature_2m` from Open-Meteo |
| `windSpeedKmh` | `double` | km/h | No | `wind_speed_10m` from Open-Meteo |
| `precipitationProbabilityPct` | `int` | % (0–100) | No | `precipitation_probability` from Open-Meteo |
| `weatherCode` | `int` | WMO code | No | `weather_code` from Open-Meteo |
| `conditionLabel` | `string` | — | No | Derived from `weatherCode` (see §3) |

**Validation rules**:
- All four raw fields (`temperature_2m`, `wind_speed_10m`, `precipitation_probability`, `weather_code`) MUST be present in the Open-Meteo response. If any is missing, the backend returns `Unknown` recommendation (FR-011).
- `precipitationProbabilityPct` is clamped to [0, 100] before evaluation.
- `conditionLabel` is derived server-side using the WMO mapping table (R-003); it is never user-supplied.

---

### 1.3 Activity (Enumeration)

Represents a supported outdoor activity type. The set is fixed for this version (spec Assumptions).

| Value | Display Name | Primary Constraint Factors |
|-------|-------------|----------------------------|
| `Running` | Running | Precipitation probability, wind speed |
| `Cycling` | Cycling | Wind speed |
| `Picnic` | Picnic | Precipitation probability, temperature |
| `Walking` | Walking | Extreme severity only |

**Validation rules**:
- The backend MUST reject any activity value outside this enum with HTTP 422 / `UNSUPPORTED_ACTIVITY` (FR-013).
- The frontend MUST only present these four values in the UI selector; free-text activity input is prohibited.

---

### 1.4 Recommendation (Output / Transient)

The output of evaluating `WeatherConditions` against an `Activity`'s suitability rules. Never persisted.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `verdict` | `Verdict` enum | One of: `Suitable`, `Caution`, `NotRecommended`, `Unknown` | FR-005, FR-011 |
| `explanation` | `string` | Non-empty; references at least one weather factor | FR-006, SC-006 |
| `activity` | `Activity` enum | Must be in the supported set | Echo of the evaluated activity |

**Verdict semantics**:
| Verdict | Meaning |
|---------|---------|
| `Suitable` | All conditions are within acceptable thresholds for the activity. |
| `Caution` | At least one condition is in the marginal (caution) range; proceed with awareness. |
| `NotRecommended` | At least one condition exceeds the blocking threshold, or extreme severity detected. |
| `Unknown` | Required weather data is missing; recommendation cannot be computed (safe fallback). |

---

## 2. Threshold Constants

Defined once in `ActivityAdvisor.Constants` (backend) and documented here as the single source of truth. All values are from the spec (Activity Suitability Thresholds section).

| Constant | Value | Unit | Used By |
|----------|-------|------|---------|
| `WindCautionKmh` | 20 | km/h | Wind ≤ 20 → Suitable tier |
| `WindBlockingKmh` | 40 | km/h | Wind > 40 → blocking |
| `WindExtremeKmh` | 60 | km/h | Wind > 60 → extreme severity override |
| `PrecipCautionPct` | 30 | % | Precipitation ≤ 30 → Suitable tier |
| `PrecipBlockingPct` | 60 | % | Precipitation > 60 → blocking |
| `TempBlockingCelsius` | 10 | °C | Temperature < 10 → blocking (Picnic only) |

> **Caution ranges** are `(Suitable threshold, Blocking threshold]` — e.g., wind 21–40 km/h is Caution.

---

## 3. WMO Code → Condition Label Mapping

Applied in `WeatherService` when constructing `WeatherConditions.conditionLabel`. Defined as a server-side lookup; the frontend displays the label verbatim.

| WMO Codes | Condition Label |
|-----------|----------------|
| 0, 1 | `Clear` |
| 2, 3, 45, 48 | `Cloudy` |
| 51, 53, 55, 56, 57, 61, 63, 65, 66, 67, 80, 81, 82, 85, 86 | `Rainy` |
| 71, 73, 75, 77 | `Snowy` |
| 95, 96, 99 | `Stormy` |
| *(any other)* | `Cloudy` *(safe fallback)* |

---

## 4. Activity Suitability Rules (Decision Matrix)

Evaluated by `ActivityAdvisorService.Evaluate()`. Rules are applied in order; the first matching rule determines the verdict and explanation.

### Rule Order (all activities): Extreme Severity Override First

| Priority | Condition | Verdict | Explanation Template |
|----------|-----------|---------|----------------------|
| 1 (override) | `weatherCode` ∈ {95, 96, 99} OR `windSpeedKmh` > 60 | `NotRecommended` | "Severe weather conditions (storm or extreme wind) make outdoor activities unsafe." |

---

### Running

| Priority | Condition | Verdict | Explanation Template |
|----------|-----------|---------|----------------------|
| 2 | `precipitationProbabilityPct` > 60 | `NotRecommended` | "Rain probability is {value}%, which makes running inadvisable." |
| 3 | `windSpeedKmh` > 40 | `NotRecommended` | "Wind speed is {value} km/h, which is too strong for running." |
| 4 | `precipitationProbabilityPct` > 30 | `Caution` | "Rain probability is {value}% — consider waterproof clothing." |
| 5 | `windSpeedKmh` > 20 | `Caution` | "Wind speed is {value} km/h — conditions are manageable but breezy." |
| 6 | *(default)* | `Suitable` | "Weather is {conditionLabel} with mild conditions — good for a run." |

---

### Cycling

| Priority | Condition | Verdict | Explanation Template |
|----------|-----------|---------|----------------------|
| 2 | `windSpeedKmh` > 40 | `NotRecommended` | "Wind speed is {value} km/h, which is unsafe for cycling." |
| 3 | `windSpeedKmh` > 20 | `Caution` | "Wind speed is {value} km/h — take care on exposed routes." |
| 4 | *(default)* | `Suitable` | "Wind conditions are calm — good for cycling." |

---

### Picnic

| Priority | Condition | Verdict | Explanation Template |
|----------|-----------|---------|----------------------|
| 2 | `precipitationProbabilityPct` > 60 | `NotRecommended` | "Rain probability is {value}%, which makes a picnic impractical." |
| 3 | `temperatureCelsius` < 10 | `NotRecommended` | "Temperature is {value}°C, which is too cold for a picnic." |
| 4 | `precipitationProbabilityPct` > 30 | `Caution` | "Rain probability is {value}% — bring a cover just in case." |
| 5 | *(default)* | `Suitable` | "Dry weather at {temp}°C — ideal for a picnic." |

---

### Walking

| Priority | Condition | Verdict | Explanation Template |
|----------|-----------|---------|----------------------|
| 2 | `windSpeedKmh` > 40 | `Caution` | "Wind speed is {value} km/h — windy but walkable with caution." |
| 3 | *(default)* | `Suitable` | "Conditions are fine for a walk." |

> **Walking note**: Only extreme severity (handled by the global override at Priority 1) triggers `NotRecommended` for walking. Wind > 40 km/h triggers `Caution` (not blocking), consistent with spec intent.

---

## 5. State Transitions

Neither City, WeatherConditions, nor Recommendation are stateful objects — all are request-scoped values computed and returned within a single API call. The only client-side state transitions are:

```
[Idle] → [Fetching weather] → [Weather loaded] → [Evaluating activity] → [Recommendation displayed]
                         ↘ [Error: city not found / service unavailable / timeout]
```

- Switching activity (User Story 4) re-evaluates locally from the cached `WeatherConditions` in React state; no additional API call is made.
- A new city search clears `WeatherConditions` and `Recommendation` and returns to `[Fetching weather]`.
