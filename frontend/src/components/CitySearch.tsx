import { useState } from 'react';
import type { FormEvent } from 'react';

interface CitySearchProps {
  isLoading: boolean;
  errorMessage: string | null;
  onSearch: (city: string) => Promise<void>;
}

export function CitySearch({ isLoading, errorMessage, onSearch }: CitySearchProps) {
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
      {errorMessage && <p className="city-search__error">{errorMessage}</p>}
    </form>
  );
}
