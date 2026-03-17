import { useState } from 'react';
import { fetchRecommendation, fetchWeather } from '../services/weatherApiClient';
import type { ActivityType as ActivityTypeValue, ErrorResponse, RecommendationResponse, WeatherResponse } from '../types/models';

type WeatherStatus = 'idle' | 'loading' | 'success' | 'error';
type RecommendationStatus = 'idle' | 'loading' | 'success' | 'error';

interface UseWeatherResult {
  status: WeatherStatus;
  weather: WeatherResponse | null;
  errorMessage: string | null;
  selectedActivity: ActivityTypeValue | null;
  recommendationStatus: RecommendationStatus;
  recommendation: RecommendationResponse | null;
  recommendationErrorMessage: string | null;
  loadWeather: (city: string) => Promise<void>;
  selectActivity: (activity: ActivityTypeValue) => Promise<void>;
}

export function useWeather(): UseWeatherResult {
  const [status, setStatus] = useState<WeatherStatus>('idle');
  const [weather, setWeather] = useState<WeatherResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [selectedActivity, setSelectedActivity] = useState<ActivityTypeValue | null>(null);
  const [recommendationStatus, setRecommendationStatus] = useState<RecommendationStatus>('idle');
  const [recommendation, setRecommendation] = useState<RecommendationResponse | null>(null);
  const [recommendationErrorMessage, setRecommendationErrorMessage] = useState<string | null>(null);

  async function loadWeather(city: string) {
    setStatus('loading');
    setErrorMessage(null);
    setSelectedActivity(null);
    setRecommendationStatus('idle');
    setRecommendation(null);
    setRecommendationErrorMessage(null);

    try {
      const result = await fetchWeather(city.trim());
      setWeather(result);
      setStatus('success');
    } catch (error: unknown) {
      setWeather(null);
      setStatus('error');
      setErrorMessage(mapWeatherError(error));
    }
  }

  async function selectActivity(activity: ActivityTypeValue) {
    setSelectedActivity(activity);
    setRecommendationStatus('loading');
    setRecommendation(null);
    setRecommendationErrorMessage(null);

    if (!weather) {
      setRecommendationStatus('error');
      setRecommendationErrorMessage('Load weather data before selecting an activity.');
      return;
    }

    try {
      const result = await fetchRecommendation(weather, activity);
      setRecommendation(result);
      setRecommendationStatus('success');
    } catch (error: unknown) {
      setRecommendationStatus('error');
      setRecommendationErrorMessage(mapRecommendationError(error));
    }
  }

  return {
    status,
    weather,
    errorMessage,
    selectedActivity,
    recommendationStatus,
    recommendation,
    recommendationErrorMessage,
    loadWeather,
    selectActivity,
  };
}

function mapRecommendationError(error: unknown): string {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const maybeResponse = (error as { response?: { data?: ErrorResponse } }).response;
    const code = maybeResponse?.data?.code;

    if (code === 'UNSUPPORTED_ACTIVITY') {
      return 'The selected activity is not supported.';
    }
  }

  return 'Unable to load recommendation.';
}

function mapWeatherError(error: unknown): string {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const maybeResponse = (error as { response?: { data?: ErrorResponse } }).response;
    const code = maybeResponse?.data?.code;

    if (code === 'CITY_NOT_FOUND') {
      return 'City not found.';
    }

    if (code === 'WEATHER_SERVICE_TIMEOUT') {
      return 'Weather request timed out. Please try again.';
    }

    if (code === 'WEATHER_SERVICE_UNAVAILABLE') {
      return 'Weather data is currently unavailable.';
    }
  }

  return 'Unable to load weather data.';
}
