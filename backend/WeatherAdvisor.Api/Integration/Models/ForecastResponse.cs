using System.Text.Json.Serialization;

namespace WeatherAdvisor.Api.Integration.Models;

public class ForecastResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double? Temperature2m { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double? WindSpeed10m { get; set; }

    [JsonPropertyName("precipitation_probability")]
    public int? PrecipitationProbability { get; set; }

    [JsonPropertyName("weather_code")]
    public int? WeatherCode { get; set; }
}
