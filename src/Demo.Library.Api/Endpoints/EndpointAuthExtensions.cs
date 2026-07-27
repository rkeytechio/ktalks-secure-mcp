using System.Security.Claims;

namespace Demo.Library.Api.Endpoints;

internal static class EndpointAuthExtensions
{
    private static readonly string[] UserIdClaimKeys =
    [
        ClaimTypes.NameIdentifier,
        "sub",
        "oid",
        "uid",
        ClaimTypes.Name
    ];

    public static string? GetCurrentUserId(this HttpContext httpContext) =>
        UserIdClaimKeys
            .Select(httpContext.User.FindFirstValue)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}