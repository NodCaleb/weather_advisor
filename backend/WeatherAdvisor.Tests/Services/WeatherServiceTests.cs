using Moq;
using WeatherAdvisor.Api.Exceptions;
using WeatherAdvisor.Api.Integration;
using WeatherAdvisor.Api.Services;

namespace WeatherAdvisor.Tests.Services;

public class WeatherServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_ReturnsMappedResponse_OnSuccess()
    {
        var clientMock = new Mock<IOpenMeteoClient>();
        clientMock
            .Setup(c => c.GetCoordinatesAsync("London"))
            .ReturnsAsync(("London", 51.5072, -0.1276));
        clientMock
            .Setup(c => c.GetCurrentWeatherAsync(51.5072, -0.1276))
            .ReturnsAsync((14.24, 18.46, 25, 3));

        var service = new WeatherService(clientMock.Object);
        var result = await service.GetWeatherAsync("London");

        Assert.Equal("London", result.City);
        Assert.Equal(14.2, result.TemperatureCelsius);
        Assert.Equal(18.5, result.WindSpeedKmh);
        Assert.Equal(25, result.PrecipitationProbabilityPct);
        Assert.Equal("Cloudy", result.ConditionLabel);
        Assert.Equal(3, result.WeatherCode);
    }

    [Fact]
    public async Task GetWeatherAsync_ThrowsCityNotFound_WhenGeocodingHasNoMatch()
    {
        var clientMock = new Mock<IOpenMeteoClient>();
        clientMock
            .Setup(c => c.GetCoordinatesAsync(It.IsAny<string>()))
            .ThrowsAsync(new CityNotFoundException("xyznotacity"));

        var service = new WeatherService(clientMock.Object);

        await Assert.ThrowsAsync<CityNotFoundException>(() => service.GetWeatherAsync("xyznotacity"));
    }

    [Fact]
    public async Task GetWeatherAsync_ThrowsTimeout_WhenForecastTimesOut()
    {
        var clientMock = new Mock<IOpenMeteoClient>();
        clientMock
            .Setup(c => c.GetCoordinatesAsync(It.IsAny<string>()))
            .ReturnsAsync(("Berlin", 52.52, 13.40));
        clientMock
            .Setup(c => c.GetCurrentWeatherAsync(52.52, 13.40))
            .ThrowsAsync(new WeatherServiceTimeoutException("timeout"));

        var service = new WeatherService(clientMock.Object);

        await Assert.ThrowsAsync<WeatherServiceTimeoutException>(() => service.GetWeatherAsync("Berlin"));
    }

    [Fact]
    public async Task GetWeatherAsync_ThrowsUnavailable_WhenForecastUnavailable()
    {
        var clientMock = new Mock<IOpenMeteoClient>();
        clientMock
            .Setup(c => c.GetCoordinatesAsync(It.IsAny<string>()))
            .ReturnsAsync(("Paris", 48.85, 2.35));
        clientMock
            .Setup(c => c.GetCurrentWeatherAsync(48.85, 2.35))
            .ThrowsAsync(new WeatherServiceUnavailableException("unavailable"));

        var service = new WeatherService(clientMock.Object);

        await Assert.ThrowsAsync<WeatherServiceUnavailableException>(() => service.GetWeatherAsync("Paris"));
    }

    [Theory]
    [InlineData(0, "Clear")]
    [InlineData(1, "Clear")]
    [InlineData(2, "Cloudy")]
    [InlineData(45, "Cloudy")]
    [InlineData(61, "Rainy")]
    [InlineData(86, "Rainy")]
    [InlineData(73, "Snowy")]
    [InlineData(77, "Snowy")]
    [InlineData(95, "Stormy")]
    [InlineData(99, "Stormy")]
    [InlineData(999, "Cloudy")]
    public async Task GetWeatherAsync_MapsWmoCodeToConditionLabel(int weatherCode, string expectedLabel)
    {
        var clientMock = new Mock<IOpenMeteoClient>();
        clientMock
            .Setup(c => c.GetCoordinatesAsync("Madrid"))
            .ReturnsAsync(("Madrid", 40.41, -3.70));
        clientMock
            .Setup(c => c.GetCurrentWeatherAsync(40.41, -3.70))
            .ReturnsAsync((20.0, 10.0, 10, weatherCode));

        var service = new WeatherService(clientMock.Object);
        var result = await service.GetWeatherAsync("Madrid");

        Assert.Equal(expectedLabel, result.ConditionLabel);
    }

    [Fact]
    public async Task GetWeatherAsync_NormalizesInput_AndReturnsEquivalentResult()
    {
        var clientMock = new Mock<IOpenMeteoClient>();
        clientMock
            .Setup(c => c.GetCoordinatesAsync("London"))
            .ReturnsAsync(("London", 51.5072, -0.1276));
        clientMock
            .Setup(c => c.GetCurrentWeatherAsync(51.5072, -0.1276))
            .ReturnsAsync((11.1, 12.2, 33, 0));

        var service = new WeatherService(clientMock.Object);
        var mixedCase = await service.GetWeatherAsync("  lOnDoN  ");
        var normalized = await service.GetWeatherAsync("London");

        Assert.Equal(normalized.City, mixedCase.City);
        Assert.Equal(normalized.TemperatureCelsius, mixedCase.TemperatureCelsius);
        Assert.Equal(normalized.WindSpeedKmh, mixedCase.WindSpeedKmh);
        Assert.Equal(normalized.PrecipitationProbabilityPct, mixedCase.PrecipitationProbabilityPct);
        Assert.Equal(normalized.ConditionLabel, mixedCase.ConditionLabel);

        clientMock.Verify(c => c.GetCoordinatesAsync("London"), Times.Exactly(2));
    }
}
