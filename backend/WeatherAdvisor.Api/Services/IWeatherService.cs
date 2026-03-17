using WeatherAdvisor.Api.Models.Responses;

namespace WeatherAdvisor.Api.Services;

public interface IWeatherService
{
    Task<WeatherResponse> GetWeatherAsync(string city);
}
