using DevHabit.Api.Database;
using DevHabit.Api.Extensions;
using DevHabit.Api.Middleware;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson()
.AddXmlSerializerFormatters();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = (context) =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options
        .UseNpgsql(
             builder.Configuration.GetConnectionString("Database"),
             npgsqloptions=>npgsqloptions
                  .MigrationsHistoryTable(HistoryRepository.DefaultTableName,Schemas.Application))
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
    {
        resourceBuilder.AddService(builder.Environment.ApplicationName);
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddAspNetCoreInstrumentation();
        tracerProviderBuilder.AddHttpClientInstrumentation();
        tracerProviderBuilder.AddNpgsql();

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
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

await app.RunAsync();
