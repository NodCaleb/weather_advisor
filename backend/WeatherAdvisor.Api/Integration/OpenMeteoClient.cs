using System.Globalization;
using System.Net.Http.Json;
using WeatherAdvisor.Api.Exceptions;
using WeatherAdvisor.Api.Integration.Models;

namespace WeatherAdvisor.Api.Integration;

public class OpenMeteoClient : IOpenMeteoClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenMeteoClient> _logger;

    public OpenMeteoClient(IHttpClientFactory httpClientFactory, ILogger<OpenMeteoClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(string DisplayName, double Latitude, double Longitude)> GetCoordinatesAsync(string cityName)
    {
        var client = _httpClientFactory.CreateClient("OpenMeteo.Geocoding");
        var url = $"/v1/search?name={Uri.EscapeDataString(cityName)}&count=1&language=en&format=json";

        try
        {
            _logger.LogDebug("Geocoding city: {City}", cityName);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Geocoding API returned {StatusCode} for city: {City}", response.StatusCode, cityName);
                throw new WeatherServiceUnavailableException($"Geocoding API returned non-success status {(int)response.StatusCode}");
            }

            var geocodingResponse = await response.Content.ReadFromJsonAsync<GeocodingResponse>();

            if (geocodingResponse?.Results is null || geocodingResponse.Results.Count == 0)
            {
                _logger.LogInformation("No geocoding results for city: {City}", cityName);
                throw new CityNotFoundException(cityName);
            }

            var result = geocodingResponse.Results[0];
            _logger.LogDebug("Resolved {City} to ({Lat},{Lon})", result.Name, result.Latitude, result.Longitude);
            return (result.Name, result.Latitude, result.Longitude);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Geocoding API timed out for city: {City}", cityName);
            throw new WeatherServiceTimeoutException("Geocoding API timed out", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Geocoding API network failure for city: {City}", cityName);
            throw new WeatherServiceUnavailableException("Geocoding API is unavailable", ex);
        }
    }

    public async Task<(double TemperatureCelsius, double WindSpeedKmh, int PrecipitationProbabilityPct, int WeatherCode)> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var client = _httpClientFactory.CreateClient("OpenMeteo.Forecast");
        var latitudeInvariant = latitude.ToString(CultureInfo.InvariantCulture);
        var longitudeInvariant = longitude.ToString(CultureInfo.InvariantCulture);
        var url = $"/v1/forecast?latitude={latitudeInvariant}&longitude={longitudeInvariant}" +
                  "&current=temperature_2m,wind_speed_10m,precipitation_probability,weather_code" +
                  "&wind_speed_unit=kmh&timezone=auto";

        try
        {
            _logger.LogDebug("Fetching forecast for ({Lat},{Lon})", latitude, longitude);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Forecast API returned {StatusCode} for ({Lat},{Lon})", response.StatusCode, latitude, longitude);
                throw new WeatherServiceUnavailableException($"Forecast API returned non-success status {(int)response.StatusCode}");
            }

            var forecastResponse = await response.Content.ReadFromJsonAsync<ForecastResponse>();
            var current = forecastResponse?.Current;

            if (current?.Temperature2m is null || current.WindSpeed10m is null ||
                current.PrecipitationProbability is null || current.WeatherCode is null)
            {
                _logger.LogWarning("Forecast API response missing required fields for ({Lat},{Lon})", latitude, longitude);
                throw new WeatherServiceUnavailableException("Forecast API response is missing required weather fields");
            }

            return (current.Temperature2m.Value, current.WindSpeed10m.Value,
                    current.PrecipitationProbability.Value, current.WeatherCode.Value);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Forecast API timed out for ({Lat},{Lon})", latitude, longitude);
            throw new WeatherServiceTimeoutException("Forecast API timed out", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Forecast API network failure for ({Lat},{Lon})", latitude, longitude);
            throw new WeatherServiceUnavailableException("Forecast API is unavailable", ex);
        }
    }
}
