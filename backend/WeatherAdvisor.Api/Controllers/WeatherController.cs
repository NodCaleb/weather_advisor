using Microsoft.AspNetCore.Mvc;
using WeatherAdvisor.Api.Exceptions;
using WeatherAdvisor.Api.Models;
using WeatherAdvisor.Api.Services;

namespace WeatherAdvisor.Api.Controllers;

[ApiController]
[Route("/")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GetWeather([FromQuery] string? city)
    {
        if (string.IsNullOrWhiteSpace(city) || city.Trim().Length > 100)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "The city query parameter is required and must be 100 characters or fewer."
            });
        }

        try
        {
            var weather = await _weatherService.GetWeatherAsync(city.Trim());
            return Ok(weather);
        }
        catch (CityNotFoundException)
        {
            return NotFound(new ErrorResponse { Code = "CITY_NOT_FOUND", Message = "City not found" });
        }
        catch (WeatherServiceTimeoutException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new ErrorResponse { Code = "WEATHER_SERVICE_TIMEOUT", Message = "Weather data request timed out." });
        }
        catch (WeatherServiceUnavailableException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse { Code = "WEATHER_SERVICE_UNAVAILABLE", Message = "Weather data is currently unavailable." });
        }
    }
}
