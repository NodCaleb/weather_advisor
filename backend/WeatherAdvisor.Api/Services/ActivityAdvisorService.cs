using System.Globalization;

namespace WeatherAdvisor.Api.Services;

public class ActivityAdvisorService : IActivityAdvisorService
{
    public const int WindCautionKmh = 20;
    public const int WindBlockingKmh = 40;
    public const int WindExtremeKmh = 60;
    public const int PrecipCautionPct = 30;
    public const int PrecipBlockingPct = 60;
    public const int TempBlockingCelsius = 10;

    public Recommendation Evaluate(WeatherConditions conditions, ActivityType activity)
    {
        if (conditions.TemperatureCelsius is null
            || conditions.WindSpeedKmh is null
            || conditions.PrecipitationProbabilityPct is null
            || conditions.WeatherCode is null)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.Unknown,
                Explanation = "Required weather data is missing, so no recommendation can be provided."
            };
        }

        var temperatureCelsius = conditions.TemperatureCelsius.Value;
        var windSpeedKmh = conditions.WindSpeedKmh.Value;
        var precipitationProbabilityPct = Math.Clamp(conditions.PrecipitationProbabilityPct.Value, 0, 100);
        var weatherCode = conditions.WeatherCode.Value;

        if (IsExtreme(weatherCode, windSpeedKmh))
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.NotRecommended,
                Explanation = "Severe weather conditions (storm or extreme wind) make outdoor activities unsafe."
            };
        }

        return activity switch
        {
            ActivityType.Running => EvaluateRunning(windSpeedKmh, precipitationProbabilityPct, weatherCode),
            ActivityType.Cycling => EvaluateCycling(windSpeedKmh),
            ActivityType.Picnic => EvaluatePicnic(temperatureCelsius, precipitationProbabilityPct),
            ActivityType.Walking => EvaluateWalking(windSpeedKmh),
            _ => new Recommendation
            {
                Verdict = RecommendationVerdict.Unknown,
                Explanation = "The selected activity is not supported."
            }
        };
    }

    private static Recommendation EvaluateRunning(double windSpeedKmh, int precipitationProbabilityPct, int weatherCode)
    {
        if (precipitationProbabilityPct > PrecipBlockingPct)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.NotRecommended,
                Explanation = $"Rain probability is {precipitationProbabilityPct}%, which makes running inadvisable."
            };
        }

        if (windSpeedKmh > WindBlockingKmh)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.NotRecommended,
                Explanation = $"Wind speed is {FormatDecimal(windSpeedKmh)} km/h, which is too strong for running."
            };
        }

        if (precipitationProbabilityPct > PrecipCautionPct)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.Caution,
                Explanation = $"Rain probability is {precipitationProbabilityPct}% - consider waterproof clothing."
            };
        }

        if (windSpeedKmh > WindCautionKmh)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.Caution,
                Explanation = $"Wind speed is {FormatDecimal(windSpeedKmh)} km/h - conditions are manageable but breezy."
            };
        }

        return new Recommendation
        {
            Verdict = RecommendationVerdict.Suitable,
            Explanation = $"Weather is {WeatherService.MapConditionLabel(weatherCode)} with mild conditions - good for a run."
        };
    }

    private static Recommendation EvaluateCycling(double windSpeedKmh)
    {
        if (windSpeedKmh > WindBlockingKmh)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.NotRecommended,
                Explanation = $"Wind speed is {FormatDecimal(windSpeedKmh)} km/h, which is unsafe for cycling."
            };
        }

        if (windSpeedKmh > WindCautionKmh)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.Caution,
                Explanation = $"Wind speed is {FormatDecimal(windSpeedKmh)} km/h - take care on exposed routes."
            };
        }

        return new Recommendation
        {
            Verdict = RecommendationVerdict.Suitable,
            Explanation = "Wind conditions are calm - good for cycling."
        };
    }

    private static Recommendation EvaluatePicnic(double temperatureCelsius, int precipitationProbabilityPct)
    {
        if (precipitationProbabilityPct > PrecipBlockingPct)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.NotRecommended,
                Explanation = $"Rain probability is {precipitationProbabilityPct}%, which makes a picnic impractical."
            };
        }

        if (temperatureCelsius < TempBlockingCelsius)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.NotRecommended,
                Explanation = $"Temperature is {FormatDecimal(temperatureCelsius)}°C, which is too cold for a picnic."
            };
        }

        if (precipitationProbabilityPct > PrecipCautionPct)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.Caution,
                Explanation = $"Rain probability is {precipitationProbabilityPct}% - bring a cover just in case."
            };
        }

        return new Recommendation
        {
            Verdict = RecommendationVerdict.Suitable,
            Explanation = $"Dry weather at {FormatDecimal(temperatureCelsius)}°C - ideal for a picnic."
        };
    }

    private static Recommendation EvaluateWalking(double windSpeedKmh)
    {
        if (windSpeedKmh > WindBlockingKmh)
        {
            return new Recommendation
            {
                Verdict = RecommendationVerdict.Caution,
                Explanation = $"Wind speed is {FormatDecimal(windSpeedKmh)} km/h - windy but walkable with caution."
            };
        }

        return new Recommendation
        {
            Verdict = RecommendationVerdict.Suitable,
            Explanation = "Conditions are fine for a walk."
        };
    }

    private static bool IsExtreme(int weatherCode, double windSpeedKmh)
    {
        return weatherCode is 95 or 96 or 99 || windSpeedKmh > WindExtremeKmh;
    }

    private static string FormatDecimal(double value)
    {
        return value.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
