import axios from 'axios';
import type { WeatherResponse, RecommendationResponse } from '../types/models';
import { ActivityType } from '../types/models';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

export async function fetchWeather(city: string): Promise<WeatherResponse> {
  const response = await apiClient.get<WeatherResponse>('/weather', {
    params: { city },
  });
  return response.data;
}

export async function fetchRecommendation(
  weather: WeatherResponse,
  activity: ActivityType,
): Promise<RecommendationResponse> {
  const response = await apiClient.post<RecommendationResponse>('/recommendation', {
    temperatureCelsius: weather.temperatureCelsius,
    windSpeedKmh: weather.windSpeedKmh,
    precipitationProbabilityPct: weather.precipitationProbabilityPct,
    weatherCode: weather.weatherCode,
    activity,
  });
  return response.data;
}
