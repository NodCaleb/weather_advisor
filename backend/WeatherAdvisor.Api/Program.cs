using WeatherAdvisor.Api.Configuration;
using WeatherAdvisor.Api.Integration;
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
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
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

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseAuthorization();
app.MapControllers();

app.Run();
