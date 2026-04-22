import { useState } from 'react';
import type { FormEvent } from 'react';

type CitySearchErrorCode =
  | 'CITY_NOT_FOUND'
  | 'WEATHER_SERVICE_TIMEOUT'
  | 'WEATHER_SERVICE_UNAVAILABLE'
  | 'VALIDATION_ERROR'
  | 'INTERNAL_ERROR'
  | 'UNKNOWN';

interface CitySearchProps {
  isLoading: boolean;
  errorCode: CitySearchErrorCode | null;
  errorMessage: string | null;
  onSearch: (city: string) => Promise<void>;
}

export function CitySearch({ isLoading, errorCode, errorMessage, onSearch }: CitySearchProps) {
  const [city, setCity] = useState('');

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    if (!city.trim()) {
      return;
    }

    await onSearch(city);
  }

  return (
    <form className="city-search" onSubmit={(event) => void handleSubmit(event)}>
      <label className="city-search__label" htmlFor="city-input">
        Enter a city
      </label>
      <div className="city-search__row">
        <input
          id="city-input"
          className="city-search__input"
          type="text"
          value={city}
          onChange={(event) => setCity(event.target.value)}
          placeholder="London"
          maxLength={100}
          disabled={isLoading}
        />
        <button className="city-search__button" type="submit" disabled={isLoading}>
          {isLoading ? 'Loading...' : 'Search'}
        </button>
      </div>
      {errorMessage && (
        <div className="city-search__error" role="alert" aria-live="polite">
          <p>{errorMessage}</p>
          {errorCode === 'CITY_NOT_FOUND' && <p>Please re-enter the city name and search again.</p>}
          {errorCode === 'WEATHER_SERVICE_TIMEOUT' && <p>The service took too long. Please retry.</p>}
          {errorCode === 'WEATHER_SERVICE_UNAVAILABLE' && <p>Try again in a few moments.</p>}
        </div>
      )}
    </form>
  );
}
