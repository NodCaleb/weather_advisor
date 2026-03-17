using System.Globalization;
using WeatherAdvisor.Api.Integration;
using WeatherAdvisor.Api.Models.Responses;

namespace WeatherAdvisor.Api.Services;

public class WeatherService : IWeatherService
{
    private static readonly TextInfo InvariantTextInfo = CultureInfo.InvariantCulture.TextInfo;
    private readonly IOpenMeteoClient _openMeteoClient;

    public WeatherService(IOpenMeteoClient openMeteoClient)
    {
        _openMeteoClient = openMeteoClient;
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        var normalizedCity = NormalizeCity(city);
        var (displayName, latitude, longitude) = await _openMeteoClient.GetCoordinatesAsync(normalizedCity);
        var (temperatureCelsius, windSpeedKmh, precipitationProbabilityPct, weatherCode) =
            await _openMeteoClient.GetCurrentWeatherAsync(latitude, longitude);

        return new WeatherResponse
        {
            City = displayName,
            TemperatureCelsius = Math.Round(temperatureCelsius, 1),
            WindSpeedKmh = Math.Round(windSpeedKmh, 1),
            PrecipitationProbabilityPct = Math.Clamp(precipitationProbabilityPct, 0, 100),
            ConditionLabel = MapConditionLabel(weatherCode),
            WeatherCode = weatherCode
        };
    }

    private static string NormalizeCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required.", nameof(city));
        }

        var trimmed = city.Trim();
        return InvariantTextInfo.ToTitleCase(trimmed.ToLowerInvariant());
    }

    internal static string MapConditionLabel(int weatherCode)
    {
        return weatherCode switch
        {
            0 or 1 => "Clear",
            2 or 3 or 45 or 48 => "Cloudy",
            51 or 53 or 55 or 56 or 57 or 61 or 63 or 65 or 66 or 67 or 80 or 81 or 82 or 85 or 86 => "Rainy",
            71 or 73 or 75 or 77 => "Snowy",
            95 or 96 or 99 => "Stormy",
            _ => "Cloudy"
        };
    }
}
