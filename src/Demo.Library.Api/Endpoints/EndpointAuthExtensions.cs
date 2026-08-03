using System.Security.Claims;

namespace Demo.Library.Api.Endpoints;

internal static class EndpointAuthExtensions
{
    private static readonly string[] UserIdClaimKeys =
    [
        "oid",
        "sub",
        ClaimTypes.NameIdentifier
    ];

    public static string? GetCurrentUserId(this HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return UserIdClaimKeys
            .Select(httpContext.User.FindFirstValue)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    public static bool HasScope(this ClaimsPrincipal user, string requiredScope)
    {
        if (string.IsNullOrWhiteSpace(requiredScope))
        {
            return false;
        }

        var scopeClaim = user.FindFirstValue("scp") ?? user.FindFirstValue("http://schemas.microsoft.com/identity/claims/scope");
        if (string.IsNullOrWhiteSpace(scopeClaim))
        {
            return false;
        }

        return scopeClaim
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(scope => string.Equals(scope, requiredScope, StringComparison.Ordinal));
    }
}