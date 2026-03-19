using WeatherAdvisor.Api.Configuration;
using WeatherAdvisor.Api.Integration;
using WeatherAdvisor.Api.Models;
using WeatherAdvisor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
var openMeteoOptions = builder.Configuration
    .GetSection(OpenMeteoOptions.SectionName)
    .Get<OpenMeteoOptions>()
    ?? new OpenMeteoOptions();

builder.Services.Configure<OpenMeteoOptions>(
    builder.Configuration.GetSection(OpenMeteoOptions.SectionName));

// --- CORS ---
const string CorsPolicyName = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// --- HttpClientFactory ---
builder.Services.AddHttpClient("OpenMeteo.Geocoding", client =>
{
    client.BaseAddress = new Uri(openMeteoOptions.GeocodingBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(openMeteoOptions.TimeoutSeconds);
});

builder.Services.AddHttpClient("OpenMeteo.Forecast", client =>
{
    client.BaseAddress = new Uri(openMeteoOptions.ForecastBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(openMeteoOptions.TimeoutSeconds);
});

// --- Dependency Injection ---
builder.Services.AddScoped<IOpenMeteoClient, OpenMeteoClient>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IActivityAdvisorService, ActivityAdvisorService>();

// --- MVC ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled exception during request processing.");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Code = "INTERNAL_ERROR",
            Message = "An unexpected error occurred."
        });
    });
});

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseAuthorization();
app.MapControllers();

app.Run();
