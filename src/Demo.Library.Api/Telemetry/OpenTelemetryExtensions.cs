using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Demo.Library.Api.Telemetry;

internal static class OpenTelemetryExtensions
{
    public static IServiceCollection AddLibraryOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddSource("*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithMetrics(builder => builder
                .AddMeter("*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithLogging()
            .UseAzureMonitor(options =>
            {
                if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
                {
                    options.ConnectionString = applicationInsightsConnectionString;
                }
            });

        return services;
    }
}