import { useState } from 'react';
import { fetchWeather } from '../services/weatherApiClient';
import type { ErrorResponse, WeatherResponse } from '../types/models';

type WeatherStatus = 'idle' | 'loading' | 'success' | 'error';

interface UseWeatherResult {
  status: WeatherStatus;
  weather: WeatherResponse | null;
  errorMessage: string | null;
  loadWeather: (city: string) => Promise<void>;
}

export function useWeather(): UseWeatherResult {
  const [status, setStatus] = useState<WeatherStatus>('idle');
  const [weather, setWeather] = useState<WeatherResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function loadWeather(city: string) {
    setStatus('loading');
    setErrorMessage(null);

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

  return {
    status,
    weather,
    errorMessage,
    loadWeather,
  };
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
