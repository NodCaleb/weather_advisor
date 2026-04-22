namespace WeatherAdvisor.Api.Exceptions;

public class WeatherServiceTimeoutException : Exception
{
    public WeatherServiceTimeoutException(string message) : base(message) { }
    public WeatherServiceTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
