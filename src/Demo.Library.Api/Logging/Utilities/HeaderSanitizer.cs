using Microsoft.Extensions.Primitives;

namespace Demo.Library.Api.Logging.Utilities;

internal static class HeaderSanitizer
{
    private static readonly HashSet<string> SensitiveHeaders =
    [
        "authorization",
        "proxy-authorization",
        "x-api-key",
        "cookie",
        "set-cookie"
    ];

    public static Dictionary<string, string> Sanitize(IHeaderDictionary headers)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            sanitized[header.Key] = IsSensitive(header.Key)
                ? "[REDACTED]"
                : FlattenHeaderValues(header.Value);
        }

        return sanitized;
    }

    private static bool IsSensitive(string headerName) =>
        SensitiveHeaders.Contains(headerName);

    private static string FlattenHeaderValues(StringValues values) =>
        string.Join(",", values.ToArray());
}