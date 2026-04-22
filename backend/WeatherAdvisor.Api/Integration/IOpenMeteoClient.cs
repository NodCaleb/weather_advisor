namespace WeatherAdvisor.Api.Integration;

public interface IOpenMeteoClient
{
    /// <summary>
    /// Resolves a city name to geographic coordinates via the Open-Meteo Geocoding API.
    /// </summary>
    /// <returns>A tuple of (DisplayName, Latitude, Longitude).</returns>
    /// <exception cref="Exceptions.CityNotFoundException">Geocoding returned no results.</exception>
    /// <exception cref="Exceptions.WeatherServiceTimeoutException">Request exceeded the configured timeout.</exception>
    /// <exception cref="Exceptions.WeatherServiceUnavailableException">API returned a non-success status or a network error occurred.</exception>
    Task<(string DisplayName, double Latitude, double Longitude)> GetCoordinatesAsync(string cityName);

    /// <summary>
    /// Fetches current weather variables from the Open-Meteo Forecast API.
    /// </summary>
    /// <returns>A tuple of (TemperatureCelsius, WindSpeedKmh, PrecipitationProbabilityPct, WeatherCode).</returns>
    /// <exception cref="Exceptions.WeatherServiceTimeoutException">Request exceeded the configured timeout.</exception>
    /// <exception cref="Exceptions.WeatherServiceUnavailableException">API returned a non-success status, a network error occurred, or required fields are missing.</exception>
    Task<(double TemperatureCelsius, double WindSpeedKmh, int PrecipitationProbabilityPct, int WeatherCode)> GetCurrentWeatherAsync(double latitude, double longitude);
}
