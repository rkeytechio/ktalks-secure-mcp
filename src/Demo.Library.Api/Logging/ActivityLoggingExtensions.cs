using Microsoft.Extensions.Options;
using Demo.Library.Api.Logging.Middleware;
using Demo.Library.Api.Logging.Options;

namespace Demo.Library.Api.Logging;

internal static class ActivityLoggingExtensions
{
    public static IServiceCollection AddActivityLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ActivityLoggingOptions>(
            configuration.GetSection(ActivityLoggingOptions.SectionName));

        services.AddTransient<RequestActivityLoggingMiddleware>();
        services.AddTransient<ResponseActivityLoggingMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseActivityLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestActivityLoggingMiddleware>();
        app.UseMiddleware<ResponseActivityLoggingMiddleware>();

        return app;
    }
}
