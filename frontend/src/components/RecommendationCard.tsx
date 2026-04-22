import type { RecommendationResponse } from '../types/models';

type RecommendationStatus = 'idle' | 'loading' | 'success' | 'error';

interface RecommendationCardProps {
  status: RecommendationStatus;
  recommendation: RecommendationResponse | null;
  errorMessage: string | null;
}

export function RecommendationCard({ status, recommendation, errorMessage }: RecommendationCardProps) {
  if (status === 'idle') {
    return (
      <section className="recommendation-card">
        <h2 className="recommendation-card__title">Recommendation</h2>
        <p className="recommendation-card__message">Select an activity to get a recommendation.</p>
      </section>
    );
  }

  if (status === 'loading') {
    return (
      <section className="recommendation-card" aria-live="polite">
        <h2 className="recommendation-card__title">Recommendation</h2>
        <p className="recommendation-card__message">Evaluating conditions...</p>
      </section>
    );
  }

  if (status === 'error') {
    return (
      <section className="recommendation-card" aria-live="polite">
        <h2 className="recommendation-card__title">Recommendation</h2>
        <p className="recommendation-card__error">{errorMessage ?? 'Unable to evaluate recommendation.'}</p>
      </section>
    );
  }

  if (!recommendation) {
    return null;
  }

  return (
    <section className="recommendation-card" aria-live="polite">
      <h2 className="recommendation-card__title">Recommendation for {recommendation.activity}</h2>
      <p className={`recommendation-card__verdict verdict-${recommendation.verdict.toLowerCase()}`}>
        {recommendation.verdict}
      </p>
      <p className="recommendation-card__message">{recommendation.explanation}</p>
    </section>
  );
}
