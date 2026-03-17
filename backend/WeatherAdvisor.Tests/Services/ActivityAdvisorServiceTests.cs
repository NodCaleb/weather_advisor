using WeatherAdvisor.Api.Services;

namespace WeatherAdvisor.Tests.Services;

public class ActivityAdvisorServiceTests
{
    private readonly ActivityAdvisorService _service = new();

    [Fact]
    public void Evaluate_Running_ReturnsSuitable_WhenConditionsAreMild()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 18,
                WindSpeedKmh = 10,
                PrecipitationProbabilityPct = 20,
                WeatherCode = 1
            },
            ActivityType.Running);

        Assert.Equal(RecommendationVerdict.Suitable, recommendation.Verdict);
        Assert.Contains("10.0", recommendation.Explanation);
        Assert.Contains("20%", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Running_ReturnsCaution_WhenPrecipitationIsInCautionRange()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 16,
                WindSpeedKmh = 8,
                PrecipitationProbabilityPct = 45,
                WeatherCode = 2
            },
            ActivityType.Running);

        Assert.Equal(RecommendationVerdict.Caution, recommendation.Verdict);
        Assert.Contains("45%", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Running_ReturnsNotRecommended_WhenPrecipitationIsHigh()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 16,
                WindSpeedKmh = 12,
                PrecipitationProbabilityPct = 72,
                WeatherCode = 3
            },
            ActivityType.Running);

        Assert.Equal(RecommendationVerdict.NotRecommended, recommendation.Verdict);
        Assert.Contains("72%", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Cycling_ReturnsSuitable_WhenWindIsCalm()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 14,
                WindSpeedKmh = 12,
                PrecipitationProbabilityPct = 20,
                WeatherCode = 2
            },
            ActivityType.Cycling);

        Assert.Equal(RecommendationVerdict.Suitable, recommendation.Verdict);
        Assert.Contains("12.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Cycling_ReturnsCaution_WhenWindIsBreezy()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 14,
                WindSpeedKmh = 28,
                PrecipitationProbabilityPct = 20,
                WeatherCode = 2
            },
            ActivityType.Cycling);

        Assert.Equal(RecommendationVerdict.Caution, recommendation.Verdict);
        Assert.Contains("28.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Cycling_ReturnsNotRecommended_WhenWindIsHigh()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 14,
                WindSpeedKmh = 45,
                PrecipitationProbabilityPct = 20,
                WeatherCode = 2
            },
            ActivityType.Cycling);

        Assert.Equal(RecommendationVerdict.NotRecommended, recommendation.Verdict);
        Assert.Contains("45.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Picnic_ReturnsSuitable_WhenWarmAndDry()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 22,
                WindSpeedKmh = 9,
                PrecipitationProbabilityPct = 10,
                WeatherCode = 1
            },
            ActivityType.Picnic);

        Assert.Equal(RecommendationVerdict.Suitable, recommendation.Verdict);
        Assert.Contains("22.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Picnic_ReturnsCaution_WhenRainChanceIsModerate()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 19,
                WindSpeedKmh = 11,
                PrecipitationProbabilityPct = 40,
                WeatherCode = 2
            },
            ActivityType.Picnic);

        Assert.Equal(RecommendationVerdict.Caution, recommendation.Verdict);
        Assert.Contains("40%", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Picnic_ReturnsNotRecommended_WhenTemperatureIsLow()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 8,
                WindSpeedKmh = 5,
                PrecipitationProbabilityPct = 15,
                WeatherCode = 2
            },
            ActivityType.Picnic);

        Assert.Equal(RecommendationVerdict.NotRecommended, recommendation.Verdict);
        Assert.Contains("8.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Walking_ReturnsSuitable_WhenNoExtremeConditions()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 13,
                WindSpeedKmh = 20,
                PrecipitationProbabilityPct = 50,
                WeatherCode = 2
            },
            ActivityType.Walking);

        Assert.Equal(RecommendationVerdict.Suitable, recommendation.Verdict);
        Assert.Contains("20.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_Walking_ReturnsCaution_WhenWindIsHigh()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 13,
                WindSpeedKmh = 43,
                PrecipitationProbabilityPct = 50,
                WeatherCode = 2
            },
            ActivityType.Walking);

        Assert.Equal(RecommendationVerdict.Caution, recommendation.Verdict);
        Assert.Contains("43.0", recommendation.Explanation);
    }

    [Theory]
    [InlineData(95)]
    [InlineData(96)]
    [InlineData(99)]
    public void Evaluate_ReturnsNotRecommended_WhenWeatherCodeIsExtreme(int weatherCode)
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 17,
                WindSpeedKmh = 12,
                PrecipitationProbabilityPct = 20,
                WeatherCode = weatherCode
            },
            ActivityType.Running);

        Assert.Equal(RecommendationVerdict.NotRecommended, recommendation.Verdict);
        Assert.Contains(weatherCode.ToString(), recommendation.Explanation);
        Assert.Contains("12.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_ReturnsNotRecommended_WhenWindIsExtreme()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 17,
                WindSpeedKmh = 61,
                PrecipitationProbabilityPct = 20,
                WeatherCode = 2
            },
            ActivityType.Cycling);

        Assert.Equal(RecommendationVerdict.NotRecommended, recommendation.Verdict);
        Assert.Contains("61.0", recommendation.Explanation);
    }

    [Fact]
    public void Evaluate_ReturnsUnknown_WhenAnyFieldIsMissing()
    {
        var recommendation = _service.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = 17,
                WindSpeedKmh = null,
                PrecipitationProbabilityPct = 20,
                WeatherCode = 2
            },
            ActivityType.Walking);

        Assert.Equal(RecommendationVerdict.Unknown, recommendation.Verdict);
    }
}
