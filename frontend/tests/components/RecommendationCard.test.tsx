// @vitest-environment jsdom

import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { RecommendationCard } from '../../src/components/RecommendationCard';

describe('RecommendationCard', () => {
  it('renders verdict label and explanation when recommendation is successful', () => {
    render(
      <RecommendationCard
        status="success"
        recommendation={{
          activity: 'Cycling',
          verdict: 'NotRecommended',
          explanation: 'Wind speed is 45.0 km/h, which is unsafe for cycling.'
        }}
        errorMessage={null}
      />
    );

    expect(screen.getByText('NotRecommended')).toBeTruthy();
    expect(screen.getByText('Wind speed is 45.0 km/h, which is unsafe for cycling.')).toBeTruthy();
  });
});
