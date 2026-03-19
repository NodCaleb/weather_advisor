var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.WeatherAdvisor_Api>("weatheradvisor-api");

builder.AddViteApp("frontend", "../../frontend")
    .WithReference(api)
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"));

builder.Build().Run();
