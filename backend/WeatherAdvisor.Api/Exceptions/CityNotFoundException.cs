namespace WeatherAdvisor.Api.Exceptions;

public class CityNotFoundException : Exception
{
    public string CityName { get; }

    public CityNotFoundException(string cityName)
        : base($"City not found: {cityName}")
    {
        CityName = cityName;
    }
}
