using Microsoft.AspNetCore.Mvc;
using WeatherAdvisor.Api.Exceptions;
using WeatherAdvisor.Api.Models;
using WeatherAdvisor.Api.Models.Requests;
using WeatherAdvisor.Api.Models.Responses;
using WeatherAdvisor.Api.Services;

namespace WeatherAdvisor.Api.Controllers;

[ApiController]
[Route("/")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IActivityAdvisorService _activityAdvisorService;

    public WeatherController(IWeatherService weatherService, IActivityAdvisorService activityAdvisorService)
    {
        _weatherService = weatherService;
        _activityAdvisorService = activityAdvisorService;
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

    [HttpPost("recommendation")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetRecommendation([FromBody] GetRecommendationRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "Request body is required."
            });
        }

        if (!Enum.TryParse<ActivityType>(request.Activity, ignoreCase: false, out var activity))
        {
            return UnprocessableEntity(new ErrorResponse
            {
                Code = "UNSUPPORTED_ACTIVITY",
                Message = "Unsupported activity value."
            });
        }

        var recommendation = _activityAdvisorService.Evaluate(
            new WeatherConditions
            {
                TemperatureCelsius = request.TemperatureCelsius,
                WindSpeedKmh = request.WindSpeedKmh,
                PrecipitationProbabilityPct = request.PrecipitationProbabilityPct,
                WeatherCode = request.WeatherCode
            },
            activity);

        return Ok(new RecommendationResponse
        {
            Activity = activity.ToString(),
            Verdict = recommendation.Verdict.ToString(),
            Explanation = recommendation.Explanation
        });
    }
}
