namespace WeatherAdvisor.Api.Models.Responses;

public class WeatherResponse
{
    public string City { get; set; } = string.Empty;
    public double TemperatureCelsius { get; set; }
    public double WindSpeedKmh { get; set; }
    public int PrecipitationProbabilityPct { get; set; }
    public string ConditionLabel { get; set; } = string.Empty;
    /// <summary>Raw WMO weather code. Included so the frontend can pass it to POST /recommendation.</summary>
    public int WeatherCode { get; set; }
}
