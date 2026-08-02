namespace Demo.Library.Api.Logging.Extensions;

internal static class HttpRequestLoggingExtensions
{
    public const string CorrelationIdHeader = "x-correlation-id";
    public const string CorrelationIdItemKey = "__Activity_CorrelationId";
    public const string ActivityRequestBodyItemKey = "__Activity_RequestBody";
    public const string ActivityRequestBodyWasTruncatedItemKey = "__Activity_RequestBodyWasTruncated";

    public static bool IsWebActivityRequest(this HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    public static string GetOrCreateCorrelationId(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            || string.IsNullOrWhiteSpace(value.ToString()))
        {
            value = Guid.NewGuid().ToString("n");
            context.Request.Headers[CorrelationIdHeader] = value.ToString();
        }

        var correlationId = value.ToString();
        context.Items[CorrelationIdItemKey] = correlationId;

        if (!context.Response.HasStarted)
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
        }

        return correlationId;
    }
}