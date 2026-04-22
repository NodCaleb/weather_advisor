// @vitest-environment jsdom

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { HomePage } from '../../src/pages/HomePage';
import { ActivityType, type RecommendationResponse, type WeatherResponse } from '../../src/types/models';
import { fetchRecommendation, fetchWeather } from '../../src/services/weatherApiClient';

vi.mock('../../src/services/weatherApiClient', () => ({
  fetchWeather: vi.fn(),
  fetchRecommendation: vi.fn(),
}));

describe('HomePage', () => {
  it('switches activity with one new recommendation call and no extra weather fetch', async () => {
    const weather: WeatherResponse = {
      city: 'London',
      temperatureCelsius: 15.2,
      windSpeedKmh: 28.5,
      precipitationProbabilityPct: 12,
      conditionLabel: 'Cloudy',
      weatherCode: 3,
    };

    const runningRecommendation: RecommendationResponse = {
      activity: ActivityType.Running,
      verdict: 'Caution',
      explanation: 'Wind speed is 28.5 km/h - conditions are manageable but breezy.',
    };

    const cyclingRecommendation: RecommendationResponse = {
      activity: ActivityType.Cycling,
      verdict: 'Caution',
      explanation: 'Wind speed is 28.5 km/h - take care on exposed routes.',
    };

    vi.mocked(fetchWeather).mockResolvedValue(weather);
    vi
      .mocked(fetchRecommendation)
      .mockResolvedValueOnce(runningRecommendation)
      .mockResolvedValueOnce(cyclingRecommendation);

    render(<HomePage />);

    fireEvent.change(screen.getByLabelText('Enter a city'), {
      target: { value: 'London' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Search' }));

    await screen.findByText('Current weather in London');
    await waitFor(() => expect(fetchWeather).toHaveBeenCalledTimes(1));

    fireEvent.click(screen.getByRole('button', { name: ActivityType.Running }));

    await waitFor(() => expect(fetchRecommendation).toHaveBeenCalledTimes(1));
    expect(fetchWeather).toHaveBeenCalledTimes(1);

    fireEvent.click(screen.getByRole('button', { name: ActivityType.Cycling }));

    await waitFor(() => expect(fetchRecommendation).toHaveBeenCalledTimes(2));
    expect(fetchWeather).toHaveBeenCalledTimes(1);

    expect(vi.mocked(fetchRecommendation).mock.calls[1]).toEqual([weather, ActivityType.Cycling]);
  });
});
