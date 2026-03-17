/** Response from GET /weather */
export interface WeatherResponse {
  city: string;
  temperatureCelsius: number;
  windSpeedKmh: number;
  precipitationProbabilityPct: number;
  conditionLabel: string;
  /** Raw WMO weather code — forwarded to POST /recommendation */
  weatherCode: number;
}

/** Response from POST /recommendation */
export interface RecommendationResponse {
  activity: string;
  verdict: string;
  explanation: string;
}

/** Error envelope returned on all non-2xx responses */
export interface ErrorResponse {
  code: string;
  message: string;
}

/** Supported outdoor activity types */
export enum ActivityType {
  Running = 'Running',
  Cycling = 'Cycling',
  Picnic = 'Picnic',
  Walking = 'Walking',
}
