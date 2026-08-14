using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
    {
        resourceBuilder.AddService(builder.Environment.ApplicationName);
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddAspNetCoreInstrumentation();
        tracerProviderBuilder.AddHttpClientInstrumentation();

    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder.AddAspNetCoreInstrumentation();
        metricsBuilder.AddHttpClientInstrumentation();
        metricsBuilder.AddRuntimeInstrumentation();
    })
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(loggingBuilder =>
{
    loggingBuilder.IncludeScopes = true;
    loggingBuilder.IncludeFormattedMessage = true;  
});

    WebApplication app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
