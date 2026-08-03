using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Demo.Library.Api.Telemetry;

internal static class OpenTelemetryExtensions
{
    public static IServiceCollection AddLibraryOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        var openTelemetryBuilder = services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddSource("*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithMetrics(builder => builder
                .AddMeter("*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithLogging();

        if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
        {
            openTelemetryBuilder.UseAzureMonitor(options =>
            {
                options.ConnectionString = applicationInsightsConnectionString;
            });
        }

        return services;
    }
}