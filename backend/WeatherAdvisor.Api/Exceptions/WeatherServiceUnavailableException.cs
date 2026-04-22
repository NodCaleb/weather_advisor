namespace WeatherAdvisor.Api.Exceptions;

public class WeatherServiceUnavailableException : Exception
{
    public WeatherServiceUnavailableException(string message) : base(message) { }
    public WeatherServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
