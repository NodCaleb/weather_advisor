namespace WeatherAdvisor.Api.Services;

public interface IActivityAdvisorService
{
    Recommendation Evaluate(WeatherConditions conditions, ActivityType activity);
}

public enum ActivityType
{
    Running,
    Cycling,
    Picnic,
    Walking
}

public enum RecommendationVerdict
{
    Suitable,
    Caution,
    NotRecommended,
    Unknown
}

public sealed class WeatherConditions
{
    public double? TemperatureCelsius { get; init; }
    public double? WindSpeedKmh { get; init; }
    public int? PrecipitationProbabilityPct { get; init; }
    public int? WeatherCode { get; init; }
}

public sealed class Recommendation
{
    public RecommendationVerdict Verdict { get; init; }
    public string Explanation { get; init; } = string.Empty;
}
