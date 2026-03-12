# API Contracts: Weather Advisor Backend

**Branch**: `001-weather-advisor-app` | **Date**: March 12, 2026  
**Base URL (development)**: `http://localhost:5000`  
**Content-Type**: `application/json` (all requests and responses)  
**Version**: v1 (no versioning prefix required for this demo)

> These contracts are the authoritative integration surface between the React frontend and the .NET backend. Per Constitution Principle III, implementation MUST match these contracts exactly. Any change requires a spec update first.

---

## Endpoints

### GET /weather

Resolves a city name to coordinates via Open-Meteo Geocoding, then fetches current weather from Open-Meteo Forecast. Returns a weather snapshot ready for display and activity evaluation.

#### Request

| Parameter | Location | Type | Required | Constraints |
|-----------|----------|------|----------|-------------|
| `city` | Query string | `string` | Yes | Non-empty, max 100 chars |

**Example**:
```
GET /weather?city=London
```

#### Success Response — 200 OK

```json
{
  "city": "London",
  "temperatureCelsius": 14.2,
  "windSpeedKmh": 18.5,
  "precipitationProbabilityPct": 25,
  "conditionLabel": "Cloudy"
}
```

| Field | Type | Notes |
|-------|------|-------|
| `city` | `string` | Resolved display name from Open-Meteo geocoding (may differ from input casing) |
| `temperatureCelsius` | `number` | Degrees Celsius, 1 decimal place |
| `windSpeedKmh` | `number` | km/h, 1 decimal place |
| `precipitationProbabilityPct` | `integer` | 0–100 |
| `conditionLabel` | `string` | One of: `Clear`, `Cloudy`, `Rainy`, `Snowy`, `Stormy` |

#### Error Responses

| HTTP Status | `code` | Trigger |
|-------------|--------|---------|
| 400 | `VALIDATION_ERROR` | Missing or empty `city` query parameter |
| 404 | `CITY_NOT_FOUND` | Geocoding returned no results |
| 503 | `WEATHER_SERVICE_UNAVAILABLE` | Open-Meteo returned a non-success HTTP status or network failure |
| 504 | `WEATHER_SERVICE_TIMEOUT` | Open-Meteo did not respond within 5 seconds |
| 500 | `INTERNAL_ERROR` | Unexpected server-side exception |

**Error envelope** (all non-2xx responses):
```json
{
  "code": "CITY_NOT_FOUND",
  "message": "City not found"
}
```

---

### POST /recommendation

Evaluates provided weather conditions against the specified activity's suitability rules and returns a verdict with explanation. No external API call is made — evaluation is purely computational.

#### Request Body

```json
{
  "temperatureCelsius": 14.2,
  "windSpeedKmh": 18.5,
  "precipitationProbabilityPct": 25,
  "weatherCode": 3,
  "activity": "Picnic"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `temperatureCelsius` | `number` | Yes | Any finite double |
| `windSpeedKmh` | `number` | Yes | ≥ 0 |
| `precipitationProbabilityPct` | `integer` | Yes | 0–100 |
| `weatherCode` | `integer` | Yes | Any non-negative integer |
| `activity` | `string` | Yes | One of: `Running`, `Cycling`, `Picnic`, `Walking` (case-sensitive) |

#### Success Response — 200 OK

```json
{
  "activity": "Picnic",
  "verdict": "Suitable",
  "explanation": "Dry weather at 14.2°C — ideal for a picnic."
}
```

| Field | Type | Notes |
|-------|------|-------|
| `activity` | `string` | Echo of the requested activity |
| `verdict` | `string` | One of: `Suitable`, `Caution`, `NotRecommended`, `Unknown` |
| `explanation` | `string` | Human-readable sentence referencing the causal weather factor(s) |

#### Error Responses

| HTTP Status | `code` | Trigger |
|-------------|--------|---------|
| 400 | `VALIDATION_ERROR` | Missing or malformed request body fields |
| 422 | `UNSUPPORTED_ACTIVITY` | `activity` value not in the supported enum |
| 500 | `INTERNAL_ERROR` | Unexpected server-side exception |

---

## Design Notes

### Frontend Interaction Flow

The frontend calls these two endpoints in sequence:

```
1. GET /weather?city={userInput}
   → on success: store WeatherResponse in component state
   → on error: display error message; do not proceed to recommendation

2. POST /recommendation
   → body: { ...weatherResponse fields, activity: selectedActivity }
   → on success: display RecommendationResponse
   → on error: display error message
```

When the user switches activity (User Story 4), only step 2 is repeated — step 1 result is reused from state. This satisfies FR-007 (no page reload, no re-fetch).

### Activity Switching — Client-Side Re-evaluation Rationale

The `POST /recommendation` endpoint is intentionally stateless and accepts weather data inline rather than a server-side session reference. This makes client-controlled activity switching trivially correct (no session management), and keeps the backend purely functional. The tradeoff is a small repeated POST on each activity switch, which is acceptable given the sub-100ms evaluation time (SC-002).

### CORS Configuration

The .NET backend MUST configure a CORS policy that allows requests from the frontend dev origin (`http://localhost:5173` for Vite default). In production, the allowed origin MUST be set via environment variable.

### Missing Data Handling

If the frontend receives a `404 CITY_NOT_FOUND` error on `GET /weather`, it MUST clear any previously displayed weather and recommendation data (FR-008). It MUST NOT call `POST /recommendation` when no valid weather response is held in state.
