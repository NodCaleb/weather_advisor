namespace WeatherAdvisor.Api.Configuration;

public class OpenMeteoOptions
{
    public const string SectionName = "OpenMeteo";

    public string GeocodingBaseUrl { get; set; } = string.Empty;
    public string ForecastBaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}
