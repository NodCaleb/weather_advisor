using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherAdvisor.Api.Exceptions;
using WeatherAdvisor.Api.Integration;

namespace WeatherAdvisor.Tests.Integration;

public class OpenMeteoClientTests
{
    [Fact]
    public async Task GetCoordinatesAsync_ThrowsUnavailable_WhenGeocodingReturns503()
    {
        var geocodingClient = CreateClientWithHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        var forecastClient = CreateClientWithHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var sut = CreateSut(geocodingClient, forecastClient);

        await Assert.ThrowsAsync<WeatherServiceUnavailableException>(() => sut.GetCoordinatesAsync("London"));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ThrowsUnavailable_WhenForecastReturns503()
    {
        var geocodingClient = CreateClientWithHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var forecastClient = CreateClientWithHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

        var sut = CreateSut(geocodingClient, forecastClient);

        await Assert.ThrowsAsync<WeatherServiceUnavailableException>(() => sut.GetCurrentWeatherAsync(51.5072, -0.1276));
    }

    [Fact]
    public async Task GetCoordinatesAsync_ThrowsTimeout_WhenGeocodingExceedsConfiguredTimeout()
    {
        var geocodingClient = CreateClientWithHandler(async cancellationToken =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"results\":[{\"name\":\"London\",\"latitude\":51.5072,\"longitude\":-0.1276}]}", Encoding.UTF8, "application/json")
            };
        });
        geocodingClient.Timeout = TimeSpan.FromMilliseconds(50);

        var forecastClient = CreateClientWithHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var sut = CreateSut(geocodingClient, forecastClient);

        await Assert.ThrowsAsync<WeatherServiceTimeoutException>(() => sut.GetCoordinatesAsync("London"));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ThrowsTimeout_WhenForecastExceedsConfiguredTimeout()
    {
        var geocodingClient = CreateClientWithHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var forecastClient = CreateClientWithHandler(async cancellationToken =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"current\":{\"temperature_2m\":15.3,\"wind_speed_10m\":12.5,\"precipitation_probability\":20,\"weather_code\":3}}", Encoding.UTF8, "application/json")
            };
        });
        forecastClient.Timeout = TimeSpan.FromMilliseconds(50);

        var sut = CreateSut(geocodingClient, forecastClient);

        await Assert.ThrowsAsync<WeatherServiceTimeoutException>(() => sut.GetCurrentWeatherAsync(51.5072, -0.1276));
    }

    private static OpenMeteoClient CreateSut(HttpClient geocodingClient, HttpClient forecastClient)
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock
            .Setup(x => x.CreateClient("OpenMeteo.Geocoding"))
            .Returns(geocodingClient);
        factoryMock
            .Setup(x => x.CreateClient("OpenMeteo.Forecast"))
            .Returns(forecastClient);

        return new OpenMeteoClient(factoryMock.Object, NullLogger<OpenMeteoClient>.Instance);
    }

    private static HttpClient CreateClientWithHandler(Func<CancellationToken, Task<HttpResponseMessage>> responseFactory)
    {
        var handler = new DelegateHttpMessageHandler(responseFactory);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com")
        };
    }

    private sealed class DelegateHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<CancellationToken, Task<HttpResponseMessage>> _responseFactory;

        public DelegateHttpMessageHandler(Func<CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responseFactory(cancellationToken);
        }
    }
}
