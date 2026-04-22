import type { WeatherResponse } from '../types/models';

interface WeatherCardProps {
  weather: WeatherResponse;
}

export function WeatherCard({ weather }: WeatherCardProps) {
  return (
    <article className="weather-card" aria-live="polite">
      <h2 className="weather-card__title">Current weather in {weather.city}</h2>
      <dl className="weather-card__grid">
        <div>
          <dt>Temperature</dt>
          <dd>{weather.temperatureCelsius.toFixed(1)} C</dd>
        </div>
        <div>
          <dt>Wind speed</dt>
          <dd>{weather.windSpeedKmh.toFixed(1)} km/h</dd>
        </div>
        <div>
          <dt>Precipitation probability</dt>
          <dd>{weather.precipitationProbabilityPct}%</dd>
        </div>
        <div>
          <dt>Condition</dt>
          <dd>{weather.conditionLabel}</dd>
        </div>
      </dl>
    </article>
  );
}
