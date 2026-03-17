import { CitySearch } from '../components/CitySearch';
import { WeatherCard } from '../components/WeatherCard';
import { useWeather } from '../hooks/useWeather';

export function HomePage() {
  const { status, weather, errorMessage, loadWeather } = useWeather();

  return (
    <main className="home-page">
      <section className="home-page__hero">
        <h1>Weather Advisor</h1>
        <p>Search a city to view current weather conditions.</p>
      </section>

      <CitySearch
        isLoading={status === 'loading'}
        errorMessage={errorMessage}
        onSearch={loadWeather}
      />

      {status === 'idle' && <p className="home-page__hint">Start by entering a city name above.</p>}
      {status === 'success' && weather && <WeatherCard weather={weather} />}
    </main>
  );
}
