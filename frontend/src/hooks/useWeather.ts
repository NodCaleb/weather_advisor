import { useEffect, useState } from 'react';
import { fetchRecommendation, fetchWeather } from '../services/weatherApiClient';
import type { ActivityType as ActivityTypeValue, ErrorResponse, RecommendationResponse, WeatherResponse } from '../types/models';

type WeatherStatus = 'idle' | 'loading' | 'success' | 'error';
type RecommendationStatus = 'idle' | 'loading' | 'success' | 'error';
type WeatherErrorCode =
  | 'CITY_NOT_FOUND'
  | 'WEATHER_SERVICE_TIMEOUT'
  | 'WEATHER_SERVICE_UNAVAILABLE'
  | 'VALIDATION_ERROR'
  | 'INTERNAL_ERROR'
  | 'UNKNOWN';

interface UseWeatherResult {
  status: WeatherStatus;
  weather: WeatherResponse | null;
  weatherErrorCode: WeatherErrorCode | null;
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
  const [weatherErrorCode, setWeatherErrorCode] = useState<WeatherErrorCode | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [selectedActivity, setSelectedActivity] = useState<ActivityTypeValue | null>(null);
  const [recommendationStatus, setRecommendationStatus] = useState<RecommendationStatus>('idle');
  const [recommendation, setRecommendation] = useState<RecommendationResponse | null>(null);
  const [recommendationErrorMessage, setRecommendationErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!weather || !selectedActivity) {
      return;
    }

    let isCancelled = false;

    setRecommendationStatus('loading');
    setRecommendation(null);
    setRecommendationErrorMessage(null);

    async function loadRecommendation() {
      try {
        const result = await fetchRecommendation(weather, selectedActivity);

        if (isCancelled) {
          return;
        }

        setRecommendation(result);
        setRecommendationStatus('success');
      } catch (error: unknown) {
        if (isCancelled) {
          return;
        }

        setRecommendationStatus('error');
        setRecommendationErrorMessage(mapRecommendationError(error));
      }
    }

    void loadRecommendation();

    return () => {
      isCancelled = true;
    };
  }, [selectedActivity, weather]);

  async function loadWeather(city: string) {
    setStatus('loading');
    setWeatherErrorCode(null);
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
      const weatherError = mapWeatherError(error);

      if (weatherError.code === 'CITY_NOT_FOUND') {
        setWeather(null);
        setRecommendation(null);
      }

      setStatus('error');
      setWeatherErrorCode(weatherError.code);
      setErrorMessage(weatherError.message);
    }
  }

  async function selectActivity(activity: ActivityTypeValue) {
    setSelectedActivity(activity);

    if (!weather) {
      setRecommendationStatus('error');
      setRecommendationErrorMessage('Load weather data before selecting an activity.');
      return;
    }
  }

  return {
    status,
    weather,
    weatherErrorCode,
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

function mapWeatherError(error: unknown): { code: WeatherErrorCode; message: string } {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const maybeResponse = (error as { response?: { data?: ErrorResponse } }).response;
    const code = maybeResponse?.data?.code;

    if (code === 'CITY_NOT_FOUND') {
      return {
        code: 'CITY_NOT_FOUND',
        message: 'City not found. Please check the spelling and try another city.',
      };
    }

    if (code === 'WEATHER_SERVICE_TIMEOUT') {
      return {
        code: 'WEATHER_SERVICE_TIMEOUT',
        message: 'Weather request timed out. Please retry.',
      };
    }

    if (code === 'WEATHER_SERVICE_UNAVAILABLE') {
      return {
        code: 'WEATHER_SERVICE_UNAVAILABLE',
        message: 'Weather data is currently unavailable.',
      };
    }

    if (code === 'VALIDATION_ERROR') {
      return {
        code: 'VALIDATION_ERROR',
        message: 'Enter a valid city name and try again.',
      };
    }

    if (code === 'INTERNAL_ERROR') {
      return {
        code: 'INTERNAL_ERROR',
        message: 'Something went wrong on the server. Please try again shortly.',
      };
    }
  }

  return {
    code: 'UNKNOWN',
    message: 'Unable to load weather data right now. Please try again.',
  };
}
