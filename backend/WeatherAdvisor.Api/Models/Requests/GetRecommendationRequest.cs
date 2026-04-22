namespace WeatherAdvisor.Api.Models.Requests;

public class GetRecommendationRequest
{
    public double? TemperatureCelsius { get; set; }
    public double? WindSpeedKmh { get; set; }
    public int? PrecipitationProbabilityPct { get; set; }
    public int? WeatherCode { get; set; }
    public string Activity { get; set; } = string.Empty;
}
